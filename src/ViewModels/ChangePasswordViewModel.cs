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
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading.Tasks;

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Localization;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class ChangePasswordViewModel : ViewModel, ICloseable
    {
        #region Constants
        // Injections:
        private readonly User user;
        private readonly ILogger logger;
        private readonly ILocalization localization;
        private readonly IPasswordChanger passwordChanger;
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
        public bool uiEnabled = true;
        public bool UIEnabled
        {
            get => uiEnabled;
            private set => Set(ref uiEnabled, value);
        }
        #endregion

        private volatile string oldPw = string.Empty;
        private volatile string newPw = string.Empty;
        private volatile string newPw2 = string.Empty;

        public ChangePasswordViewModel(User user, ILocalization localization, IPasswordChanger passwordChanger, ILogger logger)
        {
            this.user = user;
            this.logger = logger;
            this.localization = localization;
            this.passwordChanger = passwordChanger;

            SubmitCommand = new DelegateCommand(SubmitChangePassword);
            CancelCommand = new DelegateCommand(o => RequestedClose?.Invoke(null, EventArgs.Empty));
            ClosedCommand = new DelegateCommand(o => oldPw = newPw = newPw2 = null);
            OldPasswordChangedCommand = new DelegateCommand(pw => oldPw = (pw as PasswordBox)?.Password);
            NewPasswordChangedCommand = new DelegateCommand(pw => newPw = (pw as PasswordBox)?.Password);
            NewPassword2ChangedCommand = new DelegateCommand(pw => newPw2 = (pw as PasswordBox)?.Password);
        }

        private void SubmitChangePassword(object commandParam)
        {
            string totp = commandParam as string;

            if (totp.NullOrEmpty())
            {
                ErrorMessage = localization["NoTwoFactorAuthTokenProvided"];
                return;
            }

            UIEnabled = false;

            Task.Run(async () =>
            {
                if (oldPw.NullOrEmpty() || newPw.NullOrEmpty() || newPw != newPw2)
                {
                    ErrorMessage = localization["OldPasswordWrongOrNewOnesDontMatch"];
                    UIEnabled = true;
                    return;
                }

                if (newPw.Length <= 7)
                {
                    ErrorMessage = localization["PasswordTooWeakErrorMessage"];
                    UIEnabled = true;
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
                    SuccessMessage = localization["PasswordChangedSuccessfully"];
                }
                else
                {
                    ErrorMessage = localization["PasswordChangeRejectedServerSide"];
                    UIEnabled = true;
                }
            });
        }
    }
}
