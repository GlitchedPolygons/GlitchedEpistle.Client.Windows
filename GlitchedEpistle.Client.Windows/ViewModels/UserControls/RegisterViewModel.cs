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
    public class RegisterViewModel : ViewModel
    {
        #region Constants
        private readonly ISettings settings;
        private readonly IUserService userService;
        private readonly IEventAggregator eventAggregator;
        private const double ERROR_MESSAGE_INTERVAL = 7000;
        #endregion

        #region Commands
        public ICommand ImportCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand QuitCommand { get; }
        #endregion

        #region UI Bindings
        private string username = string.Empty;
        public string Username { get => username; set => Set(ref username, value); }

        private string password = string.Empty;
        public string Password { get => password; set => Set(ref password, value); }

        private string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; set => Set(ref errorMessage, value); }
        #endregion

        private bool pendingAttempt = false;
        private Timer ErrorMessageTimer { get; } = new Timer(ERROR_MESSAGE_INTERVAL) { AutoReset = true };

        public RegisterViewModel(IUserService userService, ISettings settings, IEventAggregator eventAggregator)
        {
            this.settings = settings;
            this.userService = userService;
            this.eventAggregator = eventAggregator;

            ImportCommand = new DelegateCommand(OnClickedImport);
            RegisterCommand = new DelegateCommand(OnClickedRegister);
            QuitCommand = new DelegateCommand(OnClickedQuit);

            ErrorMessageTimer.Elapsed += ErrorMessageTimer_Elapsed;
            ErrorMessageTimer.Start();
        }

        private void ErrorMessageTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ErrorMessage != null)
                ErrorMessage = null;
        }

        private void OnClickedImport(object commandParam)
        {
            // TODO: open file dialog here
        }

        private void OnClickedRegister(object commandParam)
        {
            if (pendingAttempt)
            {
                return;
            }

            pendingAttempt = true;

            // TODO: send registration post request here

            pendingAttempt = false;
        }

        private void OnClickedQuit(object commandParam)
        {
            Application.Current.Shutdown();
        }
    }
}
