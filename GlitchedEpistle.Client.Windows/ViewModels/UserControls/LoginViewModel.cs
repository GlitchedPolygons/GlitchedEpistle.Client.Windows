using System.Timers;
using System.Windows;
using System.Windows.Input;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;
using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class LoginViewModel : ViewModel
    {
        #region Constants
        private readonly ISettings settings;
        private readonly IUserService userService;
        private readonly IEventAggregator eventAggregator;
        private const double ERROR_MESSAGE_INTERVAL = 7000;
        #endregion

        #region Commands
        public ICommand LoginCommand { get; }
        public ICommand QuitCommand { get; }
        #endregion

        #region UI Bindings
        private string userId = string.Empty;
        public string UserId { get => userId; set => Set(ref userId, value); }

        private string password = string.Empty;
        public string Password { get => password; set => Set(ref password, value); }

        private string totp = string.Empty;
        public string Totp { get => totp; set => Set(ref totp, value); }

        private string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; set => Set(ref errorMessage, value); }
        #endregion

        private bool pendingAttempt = false;
        private Timer ErrorMessageTimer { get; } = new Timer(ERROR_MESSAGE_INTERVAL) { AutoReset = true };

        public LoginViewModel(IUserService userService, ISettings settings, IEventAggregator eventAggregator)
        {
            this.settings = settings;
            this.userService = userService;
            this.eventAggregator = eventAggregator;

            LoginCommand = new DelegateCommand(OnClickedLogin);
            QuitCommand = new DelegateCommand(OnClickedQuit);

            ErrorMessageTimer.Elapsed += ErrorMessageTimer_Elapsed;
            ErrorMessageTimer.Start();
        }

        private void ErrorMessageTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ErrorMessage != null)
                ErrorMessage = null;
        }

        private async void OnClickedLogin(object commandParam)
        {
            if (pendingAttempt
                || string.IsNullOrEmpty(UserId)
                || string.IsNullOrEmpty(Password)
                || string.IsNullOrEmpty(Totp))
            {
                return;
            }

            pendingAttempt = true;

            string jwt = await userService.Login(UserId, Password.SHA512(), Totp);
            if (!string.IsNullOrEmpty(jwt))
            {
                settings["Auth"] = jwt;
                settings.Save();
                eventAggregator.GetEvent<LoginSucceededEvent>().Publish();
            }
            else
            {
                ErrorMessageTimer.Stop();
                ErrorMessageTimer.Start();
                ErrorMessage = "Error! Invalid user id or password.";
            }

            pendingAttempt = false;
        }

        private void OnClickedQuit(object commandParam)
        {
            Application.Current.Shutdown();
        }
    }
}
