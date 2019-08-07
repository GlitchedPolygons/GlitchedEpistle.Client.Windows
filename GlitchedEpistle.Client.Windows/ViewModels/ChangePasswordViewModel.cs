﻿using System;
using System.Timers;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;

using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
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

        public ChangePasswordViewModel(IUserService userService, User user, ICompressionUtility gzip, IAsymmetricCryptographyRSA crypto, ILogger logger)
        {
            this.user = user;
            this.gzip = gzip;
            this.crypto = crypto;
            this.logger = logger;
            this.userService = userService;

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

                var dto = new UserChangePasswordRequestDto
                {
                    Totp = totp,
                    OldPwSHA512 = oldPw.SHA512(),
                    NewPwSHA512 = newPw.SHA512(),
                    NewPrivateKey = KeyExchangeUtility.EncryptAndCompressPrivateKey(user.PrivateKeyPem, newPw)
                };

                var requestBody = new EpistleRequestBody
                {
                    UserId = user.Id,
                    Auth = user.Token.Item2,
                    Body = gzip.Compress(JsonConvert.SerializeObject(dto))
                };

                bool success = await userService.ChangeUserPassword(requestBody.Sign(crypto, user.PrivateKeyPem));

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
