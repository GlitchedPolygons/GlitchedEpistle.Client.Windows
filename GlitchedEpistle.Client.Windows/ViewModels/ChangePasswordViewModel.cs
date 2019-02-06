using System;
using System.Windows.Input;

using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class ChangePasswordViewModel : ViewModel, ICloseable
    {
        #region Constants
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
        private string oldPw;
        public string OldPassword { get => oldPw; set => Set(ref oldPw, value); }

        private string newPw;
        public string NewPassword { get => newPw; set => Set(ref newPw, value); }

        private string newPw2;
        public string NewPassword2 { get => newPw2; set => Set(ref newPw2, value); }
        #endregion

        public ChangePasswordViewModel(IUserService userService)
        {
            this.userService = userService;
            SubmitCommand = new DelegateCommand(SubmitChangePassword);
            CancelCommand = new DelegateCommand(o => RequestedClose?.Invoke(null, null));
        }

        private void SubmitChangePassword(object commandParam)
        {
            if (string.IsNullOrEmpty(OldPassword) 
                || string.IsNullOrEmpty(NewPassword) 
                || NewPassword != NewPassword2)
            {
                return;
            }

            // TODO: change pw here using IUserService asap!
        }
    }
}
