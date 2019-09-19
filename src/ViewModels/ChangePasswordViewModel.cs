/*
    Glitched Epistle - Windows Client
    Copyright (C) 2019 Raphael Beck

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Timers;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Utilities;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.Services.CompressionUtility;
using GlitchedPolygons.Services.Cryptography.Asymmetric;

using Newtonsoft.Json;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class ChangePasswordViewModel : ViewModel, ICloseable
    {
        #region Constants
        private readonly Timer messageTimer = new Timer(7000) { AutoReset = true };

        // Injections:
        private readonly User user;
        private readonly ILogger logger;
        private readonly IUserService userService;
        private readonly ICompressionUtility gzip;
        private readonly IPasswordChanger passwordChanger;
        private readonly IAsymmetricCryptographyRSA crypto;
        #endregion

        #region Events
        public event EventHandler<EventArgs> RequestedClose;
        #endregion

        #region Commands
        public ICommand ClosedCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SubmitCommand { get; }
        public ICommand OldPasswordChangedCommand { get; }
        public ICommand NewPasswordChangedCommand { get; }
        public ICommand NewPassword2ChangedCommand { get; }
        #endregion

        #region UI Bindings
        private string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; set => Set(ref errorMessage, value); }

        private string successMessage = string.Empty;
        public string SuccessMessage { get => successMessage; set => Set(ref successMessage, value); }

        public bool uiEnabled = true;
        public bool UIEnabled
        {
            get => uiEnabled;
            private set => Set(ref uiEnabled, value);
        }
        #endregion

        private string oldPw = string.Empty;
        private string newPw = string.Empty;
        private string newPw2 = string.Empty;

        public ChangePasswordViewModel(IUserService userService, User user, ICompressionUtility gzip, IPasswordChanger passwordChanger, IAsymmetricCryptographyRSA crypto, ILogger logger)
        {
            this.user = user;
            this.gzip = gzip;
            this.crypto = crypto;
            this.logger = logger;
            this.userService = userService;
            this.passwordChanger = passwordChanger;

            SubmitCommand = new DelegateCommand(SubmitChangePassword);
            CancelCommand = new DelegateCommand(o => RequestedClose?.Invoke(null, EventArgs.Empty));
            ClosedCommand = new DelegateCommand(o => oldPw = newPw = newPw2 = null);
            OldPasswordChangedCommand = new DelegateCommand(pw => oldPw = (pw as PasswordBox)?.Password);
            NewPasswordChangedCommand = new DelegateCommand(pw => newPw = (pw as PasswordBox)?.Password);
            NewPassword2ChangedCommand = new DelegateCommand(pw => newPw2 = (pw as PasswordBox)?.Password);

            messageTimer.Elapsed += (_, __) => ErrorMessage = SuccessMessage = null;
            messageTimer.Start();
        }

        private void ResetMessages()
        {
            messageTimer.Stop();
            messageTimer.Start();
            ErrorMessage = SuccessMessage = null;
        }

        private void SubmitChangePassword(object commandParam)
        {
            string totp = commandParam as string;

            if (totp.NullOrEmpty())
            {
                ResetMessages();
                ErrorMessage = "No 2FA token provided - please take security seriously and authenticate your request!";
                return;
            }

            UIEnabled = false;

            Task.Run(async () =>
            {
                if (oldPw.NullOrEmpty() || newPw.NullOrEmpty() || newPw != newPw2)
                {
                    ExecUI(() =>
                    {
                        ResetMessages();
                        ErrorMessage = "The old password is wrong or the new ones don't match.";
                        UIEnabled = true;
                    });
                    return;
                }

                if (newPw.Length < 7)
                {
                    ExecUI(() =>
                    {
                        ResetMessages();
                        ErrorMessage = "Your password is too weak; make sure that it has at least >7 characters!";
                        UIEnabled = true;
                    });
                    return;
                }

                if (user.PrivateKeyPem.NullOrEmpty())
                {
                    string msg = "The user's in-memory private key seems to be null or empty; can't change passwords without re-encrypting a new copy of the user key!";
                    logger?.LogError(msg);
                    throw new ApplicationException(msg);
                }

                bool success = await passwordChanger.ChangePassword(oldPw, newPw, totp);

                if (success)
                {
                    oldPw = newPw = newPw2 = totp = null;
                    ExecUI(() =>
                    {
                        ResetMessages();
                        SuccessMessage = "Congrats! Your password's been changed successfully.";
                    });
                }
                else
                {
                    ExecUI(() =>
                    {
                        ResetMessages();
                        ErrorMessage = "Password change request rejected server-side: perhaps invalid 2FA token?";
                        UIEnabled = true;
                    });
                }
            });
        }
    }
}
