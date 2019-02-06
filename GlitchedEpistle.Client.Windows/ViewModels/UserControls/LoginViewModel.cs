using System;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class LoginViewModel : ViewModel
    {
        #region Constants
        private readonly User user;
        private readonly ISettings settings;
        private readonly IUserService userService;
        private readonly IEventAggregator eventAggregator;
        private readonly Timer errorMessageTimer = new Timer(ERROR_MESSAGE_INTERVAL_MS) { AutoReset = true };
        private const double ERROR_MESSAGE_INTERVAL_MS = 7000;
        #endregion

        #region Commands
        public ICommand LoginCommand { get; }
        public ICommand QuitCommand { get; }
        public ICommand PasswordChangedCommand { get; }
        #endregion

        #region UI Bindings
        private string userId = string.Empty;
        public string UserId { get => userId; set => Set(ref userId, value); }

        private string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; set => Set(ref errorMessage, value); }
        #endregion

        private string password;
        private int failedAttempts = 0;
        private bool pendingAttempt = false;

        public LoginViewModel(IUserService userService, ISettings settings, IEventAggregator eventAggregator, User user)
        {
            this.user = user;
            this.settings = settings;
            this.userService = userService;
            this.eventAggregator = eventAggregator;

            QuitCommand = new DelegateCommand(OnClickedQuit);
            LoginCommand = new DelegateCommand(OnClickedLogin);
            PasswordChangedCommand = new DelegateCommand(OnChangedPassword);

            errorMessageTimer.Elapsed += (_, __) => ErrorMessage = null;
            errorMessageTimer.Start();
        }

        private void OnChangedPassword(object commandParam)
        {
            if (commandParam is PasswordBox passwordBox)
            {
                password = passwordBox.Password;
            }
        }

        private async void OnClickedLogin(object commandParam)
        {
            string totp = commandParam as string;

            if (pendingAttempt
                || string.IsNullOrEmpty(UserId)
                || string.IsNullOrEmpty(password)
                || string.IsNullOrEmpty(totp))
            {
                return;
            }

            pendingAttempt = true;

            string jwt = await userService.Login(UserId, password.SHA512(), totp);
            if (!string.IsNullOrEmpty(jwt))
            {
                failedAttempts = 0;
                user.Token = new Tuple<DateTime, string>(DateTime.UtcNow, jwt);
                eventAggregator.GetEvent<LoginSucceededEvent>().Publish();
            }
            else
            {
                failedAttempts++;
                errorMessageTimer.Stop();
                errorMessageTimer.Start();
                ErrorMessage = "Error! Invalid user id, password or 2FA.";
                if (failedAttempts > 3) ErrorMessage += "\nNote that if your credentials are correct but login fails nonetheless, it might be that you're locked out due to too many failed attempts!\nPlease try again in 15 minutes.";
            }

            pendingAttempt = false;
        }

        private void OnClickedQuit(object commandParam)
        {
            Application.Current.Shutdown();
        }

        ~LoginViewModel()
        {
            password = null;
        }
    }
}
