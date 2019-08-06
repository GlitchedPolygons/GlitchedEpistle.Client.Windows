using System;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using GlitchedPolygons.GlitchedEpistle.Client.Utilities;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.ServerHealth;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.Services.Cryptography.Symmetric;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class LoginViewModel : ViewModel
    {
        #region Constants
        private const double ERROR_MESSAGE_INTERVAL_MS = 7000;
        private readonly Timer errorMessageTimer = new Timer(ERROR_MESSAGE_INTERVAL_MS) { AutoReset = true };

        // Injections:
        private readonly User user;
        private readonly ILogger logger;
        private readonly ISettings settings;
        private readonly IUserService userService;
        private readonly IEventAggregator eventAggregator;
        private readonly IServerConnectionTest connectionTest;
        #endregion

        #region Commands
        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand QuitCommand { get; }
        public ICommand PasswordChangedCommand { get; }
        #endregion

        #region UI Bindings
        private string userId = string.Empty;
        public string UserId { get => userId; set => Set(ref userId, value); }

        private string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; set => Set(ref errorMessage, value); }

        private bool uiEnabled = true;
        public bool UIEnabled { get => uiEnabled; set => Set(ref uiEnabled, value); }
        #endregion

        private volatile string password;
        private volatile int failedAttempts;
        private volatile bool pendingAttempt;

        public LoginViewModel(IUserService userService, ISettings settings, IEventAggregator eventAggregator, User user, IServerConnectionTest connectionTest, ILogger logger)
        {
            this.user = user;
            this.logger = logger;
            this.settings = settings;
            this.userService = userService;
            this.connectionTest = connectionTest;
            this.eventAggregator = eventAggregator;

            QuitCommand = new DelegateCommand(OnClickedQuit);
            LoginCommand = new DelegateCommand(OnClickedLogin);
            RegisterCommand = new DelegateCommand(_ => eventAggregator.GetEvent<ClickedRegisterButtonEvent>().Publish());

            // Bind the password box to the password field.
            PasswordChangedCommand = new DelegateCommand(o => password = (o as PasswordBox)?.Password);

            errorMessageTimer.Elapsed += (_, __) => ErrorMessage = null;
            errorMessageTimer.Start();
        }

        private void OnClickedLogin(object commandParam)
        {
            string totp = commandParam as string;

            if (pendingAttempt || UserId.NullOrEmpty() || password.NullOrEmpty() || totp.NullOrEmpty())
            {
                return;
            }

            pendingAttempt = true;
            UIEnabled = false;

            Task.Run(async () =>
            {
                if (!await connectionTest.TestConnection())
                {
                    Application.Current?.Dispatcher?.Invoke(() => 
                    {
                        pendingAttempt = false;
                        UIEnabled = true;
                        var errorView = new InfoDialogView { DataContext = new InfoDialogViewModel { OkButtonText = "Okay :/", Text = "ERROR: The Glitched Epistle server is unresponsive. It might be under maintenance, please try again later! Sorry.", Title = "Epistle Server Unresponsive" } };
                        errorView.ShowDialog();
                    });
                    return;
                }
                
                UserLoginSuccessResponseDto response = await userService.Login(new UserLoginRequestDto
                {
                    UserId = UserId,
                    PasswordSHA512 = password.SHA512(),
                    Totp = totp
                });

                if (response is null || response.Auth.NullOrEmpty() || response.PrivateKey.NullOrEmpty())
                {
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        failedAttempts++;
                        errorMessageTimer.Stop();
                        errorMessageTimer.Start();

                        ErrorMessage = "Error! Invalid user id, password or 2FA.";
                        if (failedAttempts > 3)
                        {
                            ErrorMessage += "\nNote that if your credentials are correct but login fails nonetheless, it might be that you're locked out due to too many failed attempts!\nPlease try again in 15 minutes.";
                        }
                    });
                }
                else
                {
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        failedAttempts = 0;

                        try
                        {
                            user.Id = settings[nameof(UserId)] = UserId;
                            settings.Save();

                            user.PublicKeyPem = KeyExchangeUtility.DecompressPublicKey(response.PublicKey);
                            user.PrivateKeyPem = KeyExchangeUtility.DecompressAndDecryptPrivateKey(response.PrivateKey, password);
                            user.Token = new Tuple<DateTime, string>(DateTime.UtcNow, response.Auth);

                            eventAggregator.GetEvent<LoginSucceededEvent>().Publish();
                        }
                        catch (Exception e)
                        {
                            failedAttempts++;
                            logger.LogError(e.ToString());
                            errorMessageTimer.Stop();
                            errorMessageTimer.Start();
                            ErrorMessage = "Unexpected ERROR! Login succeeded server-side, but the returned response couldn't be handled properly (probably key decryption failure). Thrown exception was logged to the log file.";
                        }
                    });
                }

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    pendingAttempt = false;
                    UIEnabled = true;
                });
            });
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
