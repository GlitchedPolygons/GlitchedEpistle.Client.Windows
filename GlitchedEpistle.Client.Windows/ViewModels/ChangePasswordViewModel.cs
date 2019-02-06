using System;
using System.Timers;
using System.Windows.Input;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
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
        #endregion

        #region Events
        public event EventHandler<EventArgs> RequestedClose;
        #endregion

        #region Commands
        public ICommand CancelCommand { get; }
        public ICommand SubmitCommand { get; }
        #endregion

        #region UI Bindings
        private string oldPw = string.Empty;
        public string OldPassword { get => oldPw; set => Set(ref oldPw, value); }

        private string newPw = string.Empty;
        public string NewPassword { get => newPw; set => Set(ref newPw, value); }

        private string newPw2 = string.Empty;
        public string NewPassword2 { get => newPw2; set => Set(ref newPw2, value); }

        private string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; set => Set(ref errorMessage, value); }

        private string successMessage = string.Empty;
        public string SuccessMessage { get => successMessage; set => Set(ref successMessage, value); }
        #endregion

        public ChangePasswordViewModel(IUserService userService, User user)
        {
            this.user = user;
            this.userService = userService;

            SubmitCommand = new DelegateCommand(SubmitChangePassword);
            CancelCommand = new DelegateCommand(o => RequestedClose?.Invoke(null, EventArgs.Empty));

            messageTimer.Elapsed += (_, __) => { ErrorMessage = null; SuccessMessage = null; };
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

            if (string.IsNullOrEmpty(totp))
            {
                ResetMessages();
                ErrorMessage = "No 2FA token provided - please take security seriously and authenticate your request!";
                return;
            }

            bool totpValid = await userService.Validate2FA(user.Id, totp);

            if (!totpValid)
            {
                ResetMessages();
                ErrorMessage = "Two-Factor Authentication failed! Password change request rejected.";
                return;
            }

            if (string.IsNullOrEmpty(OldPassword) || string.IsNullOrEmpty(NewPassword) || NewPassword != NewPassword2)
            {
                ResetMessages();
                ErrorMessage = "The old password is wrong or the new ones don't match.";
                return;
            }

            if (NewPassword.Length < 7)
            {
                ResetMessages();
                ErrorMessage = "Your password is too weak; make sure that it has at least >7 characters!";
                return;
            }

            bool success = await userService.ChangeUserPassword(user.Id, user.Token.Item2, OldPassword.SHA512(), NewPassword.SHA512());

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
        }
    }
}
