using System;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Input;

using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Symmetric;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class ChangePasswordViewModel : ViewModel, ICloseable
    {
        #region Constants
        private readonly Timer messageTimer = new Timer(7000) { AutoReset = true };

        // Injections:
        private readonly User user;
        private readonly IUserService userService;
        private readonly ISymmetricCryptography crypto;
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

        public ChangePasswordViewModel(IUserService userService, User user, ISymmetricCryptography crypto)
        {
            this.user = user;
            this.crypto = crypto;
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

        private async void SubmitChangePassword(object commandParam)
        {
            string totp = commandParam as string;

            if (totp.NullOrEmpty())
            {
                ResetMessages();
                ErrorMessage = "No 2FA token provided - please take security seriously and authenticate your request!";
                return;
            }

            UIEnabled = false;

            bool totpValid = await userService.Validate2FA(user.Id, totp);

            if (!totpValid)
            {
                ResetMessages();
                ErrorMessage = "Two-Factor Authentication failed! Password change request rejected.";
                UIEnabled = true;
                return;
            }

            if (oldPw.NullOrEmpty() || newPw.NullOrEmpty() || newPw != newPw2)
            {
                ResetMessages();
                ErrorMessage = "The old password is wrong or the new ones don't match.";
                UIEnabled = true;
                return;
            }

            if (newPw.Length < 7)
            {
                ResetMessages();
                ErrorMessage = "Your password is too weak; make sure that it has at least >7 characters!";
                UIEnabled = true;
                return;
            }

            var dto = new UserChangePasswordRequestDto
            {
                UserId = user.Id,
                Auth = user.Token.Item2,
                OldPwSHA512 = oldPw.SHA512(),
                NewPwSHA512 = newPw.SHA512(),
                NewPrivateKeyXmlEncryptedBytesBase64 = crypto.EncryptRSAParameters(user.PrivateKey, newPw)
            };

            bool success = await userService.ChangeUserPassword(dto);

            if (success)
            {
                ResetMessages();
                SuccessMessage = "Congrats! Your password's been changed successfully.";
            }
            else
            {
                ResetMessages();
                ErrorMessage = "An unknown error occurred whilst trying to change your password.";
            }

            oldPw = newPw = newPw2 = totp = null;
        }
    }
}
