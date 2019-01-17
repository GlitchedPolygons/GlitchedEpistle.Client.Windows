using System.Windows;
using System.Windows.Input;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class LoginViewModel : ViewModel
    {
        #region Commands
        public ICommand LoginCommand { get; }
        public ICommand QuitCommand { get; }
        #endregion

        #region UI Bindings
        private string userId = string.Empty;
        public string UserId { get => userId; set => Set(ref userId, value); }

        private string password = string.Empty;
        public string Password { get => password; set => Set(ref password, value); }
        #endregion

        public LoginViewModel()
        {
            LoginCommand = new DelegateCommand(OnClickedLogin);
            QuitCommand = new DelegateCommand(OnClickedQuit);
        }

        private void OnClickedLogin(object commandParam)
        {

        }

        private void OnClickedQuit(object commandParam)
        {
            Application.Current.Shutdown();
        }
    }
}
