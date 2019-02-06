﻿using System;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Threading.Tasks;

using GlitchedPolygons.ExtensionMethods.RSAXmlPemStringConverter;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.UserControls;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;
using GlitchedPolygons.Services.MethodQ;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class MainViewModel : ViewModel
    {
        #region Constants
        public const double SIDEBAR_MIN_WIDTH = 345;
        public const double SIDEBAR_MAX_WIDTH = 666;
        public const double MAIN_WINDOW_MIN_WIDTH = 800;
        public const double MAIN_WINDOW_MIN_HEIGHT = 480;

        // Injections:
        private readonly User user;
        private readonly IMethodQ methodQ;
        private readonly ISettings settings;
        private readonly IUserService userService;
        private readonly IWindowFactory windowFactory;
        private readonly IEventAggregator eventAggregator;
        private readonly IViewModelFactory viewModelFactory;
        private static readonly TimeSpan AUTH_REFRESH_INTERVAL = TimeSpan.FromMinutes(15);
        #endregion

        #region Commands
        public ICommand ClosedCommand { get; }
        public ICommand HelpButtonCommand { get; }
        public ICommand ResetWindowButtonCommand { get; }
        public ICommand SettingsButtonCommand { get; }
        public ICommand CreateConvoButtonCommand { get; }
        public ICommand JoinConvoButtonCommand { get; }
        public ICommand ChangePasswordButtonCommand { get; }
        public ICommand LogoutButtonCommand { get; }
        #endregion

        #region UI Bindings
        private bool uiEnabled;
        public bool UIEnabled { get => uiEnabled; set => Set(ref uiEnabled, value); }

        private string username = SettingsViewModel.DEFAULT_USERNAME;
        public string Username { get => username; set => Set(ref username, value); }

        private string userId = string.Empty;
        public string UserId { get => userId; set => Set(ref userId, value); }

        private double sidebarWidth = SIDEBAR_MIN_WIDTH;
        public double SidebarWidth { get => sidebarWidth; set => Set(ref sidebarWidth, value); }

        private double sidebarMinWidth = SIDEBAR_MIN_WIDTH;
        public double SidebarMinWidth { get => sidebarMinWidth; set => Set(ref sidebarMinWidth, value); }

        public double SidebarMaxWidth => SIDEBAR_MAX_WIDTH;

        private double mainWindowWidth = MAIN_WINDOW_MIN_WIDTH;
        public double MainWindowWidth { get => mainWindowWidth; set => Set(ref mainWindowWidth, value); }

        private double mainWindowHeight = MAIN_WINDOW_MIN_HEIGHT;
        public double MainWindowHeight { get => mainWindowHeight; set => Set(ref mainWindowHeight, value); }

        private WindowState windowState = WindowState.Normal;
        public WindowState WindowState { get => windowState; set => Set(ref windowState, value); }

        private string progressBarTooltip = "Subscription expires the 24th of December, 2018 at 15:30. Click to extend now!";
        public string ProgressBarTooltip { get => progressBarTooltip; set => Set(ref progressBarTooltip, value); }

        private double progressBarValue = 100;
        public double ProgressBarValue { get => progressBarValue; set => Set(ref progressBarValue, value); }

        private Control mainControl;
        public Control MainControl { get => mainControl; set => Set(ref mainControl, value); }
        #endregion

        private bool reset = false;
        private int? scheduledAuthRefresh = null, scheduledExpirationDialog = null;

        public MainViewModel(ISettings settings, IEventAggregator eventAggregator, IUserService userService, IWindowFactory windowFactory, IViewModelFactory viewModelFactory, User user, IMethodQ methodQ)
        {
            this.user = user;
            this.methodQ = methodQ;
            this.settings = settings;
            this.userService = userService;
            this.windowFactory = windowFactory;
            this.viewModelFactory = viewModelFactory;
            this.eventAggregator = eventAggregator;

            ClosedCommand = new DelegateCommand(OnClosed);
            SettingsButtonCommand = new DelegateCommand(OnClickedSettingsIcon);
            ResetWindowButtonCommand = new DelegateCommand(OnClickedResetWindowIcon);
            HelpButtonCommand = new DelegateCommand(OnClickedHelpIcon);
            CreateConvoButtonCommand = new DelegateCommand(OnClickedCreateConvo);
            ChangePasswordButtonCommand = new DelegateCommand(OnClickedChangePassword);
            JoinConvoButtonCommand = new DelegateCommand(OnClickedJoinConvo);
            LogoutButtonCommand = new DelegateCommand(OnClickedLogout);

            // Update the username label on the main window when that setting has changed.
            eventAggregator.GetEvent<UsernameChangedEvent>().Subscribe(newUsername => Username = newUsername);

            // Update the main control when login was successful.
            eventAggregator.GetEvent<LoginSucceededEvent>().Subscribe(OnLoginSuccessful);

            // Show the 2FA secret QR code + list of backup codes after the registration form has been submitted successfully.
            eventAggregator.GetEvent<UserCreationSucceededEvent>().Subscribe(OnUserCreationSuccessful);

            // After a successful user creation, show the login screen.
            eventAggregator.GetEvent<UserCreationVerifiedEvent>().Subscribe(OnUserCreationVerified);

            // If the user agreed to delete all of his data on the local machine, respect his will
            // and get rid of everything (even preventing new settings to be written out on app shutdown too).
            eventAggregator.GetEvent<ResetConfirmedEvent>().Subscribe(OnConfirmedTotalReset);

            // When the user redeemed a coupon, update the account's remaining time bar in main menu.
            eventAggregator.GetEvent<CouponRedeemedEvent>().Subscribe(OnCouponRedeemedSuccessfully);

            // Load up the settings on startup.
            if (settings.Load())
            {
                Username = settings[nameof(SettingsViewModel.Username), SettingsViewModel.DEFAULT_USERNAME];
                UserId = user.Id = settings[nameof(UserId)];

                Enum.TryParse<WindowState>(settings[nameof(WindowState), WindowState.Normal.ToString()], out var loadedWindowState);
                WindowState = loadedWindowState;

                MainWindowWidth = Math.Abs(settings[nameof(MainWindowWidth), MAIN_WINDOW_MIN_WIDTH]);
                MainWindowHeight = Math.Abs(settings[nameof(MainWindowHeight), MAIN_WINDOW_MIN_HEIGHT]);

                double w = Math.Abs(settings[nameof(SidebarWidth), SIDEBAR_MIN_WIDTH]);
                SidebarWidth = w < SIDEBAR_MIN_WIDTH ? SIDEBAR_MIN_WIDTH : w > SIDEBAR_MAX_WIDTH ? SIDEBAR_MIN_WIDTH : w;

                if (File.Exists(Paths.PRIVATE_KEY_PATH))
                {
                    string keyXml = File.ReadAllText(Paths.PRIVATE_KEY_PATH).PemToXml();
                    user.PrivateKey = RSAParametersExtensions.FromXml(keyXml);
                }
            }

            if (string.IsNullOrEmpty(UserId))
            {
                ShowRegisterControl();
            }
            else
            {
                ShowLoginControl();
            }
        }

        #region MainControl

        private void ShowLoginControl()
        {
            var viewModel = viewModelFactory.Create<LoginViewModel>();
            viewModel.UserId = UserId;
            MainControl = new LoginView { DataContext = viewModel };
        }

        private void ShowRegisterControl()
        {
            var viewModel = viewModelFactory.Create<UserCreationViewModel>();
            MainControl = new UserCreationView { DataContext = viewModel };
        }

        private void ShowExpiredControl()
        {
            var viewModel = viewModelFactory.Create<ExpiredViewModel>();
            MainControl = new ExpiredView { DataContext = viewModel };
        }

        private void ShowExpirationReminderControl()
        {
            var viewModel = viewModelFactory.Create<ExpirationReminderViewModel>();
            MainControl = new ExpirationReminderView { DataContext = viewModel };
        }

        private void ShowCouponRedeemedSuccessfullyControl()
        {
            var viewModel = viewModelFactory.Create<CouponRedeemedSuccessfullyViewModel>();
            MainControl = new CouponRedeemedSuccessfullyView { DataContext = viewModel };
        }

        #endregion

        private async void UpdateUserExp()
        {
            // Gather user expiration UTC from server.
            DateTime utcNow = DateTime.UtcNow;
            DateTime? exp = await userService.GetUserExpirationUTC(user.Id);

            if (!exp.HasValue)
            {
                return;
            }

            // Update the UI accordingly (blue progress bar + tooltip).
            user.ExpirationUTC = exp.Value;
            bool expired = utcNow > user.ExpirationUTC;
            ProgressBarValue = expired ? 0 : (user.ExpirationUTC - utcNow).TotalHours * 100.0d / 720.0d;
            ProgressBarTooltip = $"Subscription {(expired ? "expired since" : "expires")} {user.ExpirationUTC:U}. Click to extend now!";

            // Schedule expiration dialog / extension prompt
            if (expired)
            {
                ShowExpiredControl();
            }
            else
            {
                if (scheduledExpirationDialog.HasValue)
                {
                    methodQ.Cancel(scheduledExpirationDialog.Value);
                }

                scheduledExpirationDialog = methodQ.Schedule(ShowExpiredControl, user.ExpirationUTC);

                TimeSpan remainingTime = user.ExpirationUTC - utcNow;
                if (remainingTime < TimeSpan.FromDays(5))
                {
                    ShowExpirationReminderControl();
                }
            }

            // When the user account is expired,
            // the UI should be inactive (and inoperable).
            UIEnabled = !expired;
        }

        private void OnClosed(object commandParam)
        {
            // Don't save out anything if the app's been reset.
            if (reset)
            {
                var dir = new DirectoryInfo(Paths.ROOT_DIRECTORY);
                if (dir.Exists)
                {
                    dir.DeleteRecursively();
                }
                Application.Current.Shutdown();
                return;
            }

            // Save the window's state before termination.
            settings.Load();
            var c = CultureInfo.InvariantCulture;
            settings[nameof(WindowState)] = WindowState.ToString();
            settings[nameof(MainWindowWidth)] = ((int)MainWindowWidth).ToString(c);
            settings[nameof(MainWindowHeight)] = ((int)MainWindowHeight).ToString(c);
            settings[nameof(SidebarWidth)] = ((int)SidebarWidth).ToString(c);
            settings.Save();

            Application.Current.Shutdown();
        }

        private void OnLoginSuccessful()
        {
            MainControl = null;
            UIEnabled = true;
            settings.Load();
            Username = settings[nameof(Username), SettingsViewModel.DEFAULT_USERNAME];

            if (!scheduledAuthRefresh.HasValue)
            {
                scheduledAuthRefresh = methodQ.Schedule(OnRefreshAuth, AUTH_REFRESH_INTERVAL);
            }

            UpdateUserExp();
        }

        private void OnCouponRedeemedSuccessfully()
        {
            UpdateUserExp();
            ShowCouponRedeemedSuccessfullyControl();
        }

        private void OnUserCreationSuccessful(UserCreationResponse userCreationResponse)
        {
            settings.Load();
            Username = settings[nameof(Username), SettingsViewModel.DEFAULT_USERNAME];
            settings[nameof(UserId)] = UserId = user.Id = userCreationResponse.Id;
            settings.Save();

            // Create QR code containing the Authy setup link and open the RegistrationSuccessfulView.
            IBarcodeWriter<WriteableBitmap> qrWriter = new BarcodeWriter<WriteableBitmap>
            {
                Format = BarcodeFormat.QR_CODE,
                Renderer = new WriteableBitmapRenderer(),
                Options = new EncodingOptions { Height = 200, Width = 200, Margin = 0 },
            };

            var viewModel = viewModelFactory.Create<UserCreationSuccessfulViewModel>();
            viewModel.QR = qrWriter.Write($"otpauth://totp/GlitchedEpistle:{userCreationResponse.Id}?secret={userCreationResponse.TotpSecret}");
            viewModel.BackupCodes = userCreationResponse.TotpEmergencyBackupCodes;

            MainControl = new UserCreationSuccessfulView { DataContext = viewModel };
        }

        private void OnUserCreationVerified()
        {
            ShowLoginControl();
        }

        private void OnClickedCreateConvo(object commandParam)
        {
            // TODO: implement create convo dialog
        }

        private void OnClickedChangePassword(object commandParam)
        {
            // TODO: implement change pw dialog
        }

        private void OnClickedJoinConvo(object commandParam)
        {
            // TODO: implement join convo dialog
        }

        private void OnClickedLogout(object commandParam)
        {
            if (MainControl is LoginView)
            {
                return;
            }

            Logout();
        }

        private void OnClickedSettingsIcon(object commandParam)
        {
            var settingsView = windowFactory.Create<SettingsView>(true);

            // When opening views that only exist one at a time,
            // it's important not to recreate the viewmodel every time,
            // as that would override any changes made.
            // Therefore, check if the view already has a data context that isn't null.
            if (settingsView.DataContext is null)
            {
                settingsView.DataContext = viewModelFactory.Create<SettingsViewModel>();
            }

            settingsView.Show();
            settingsView.Activate();
        }

        private void OnClickedHelpIcon(object commandParam)
        {
            var helpView = windowFactory.Create<HelpView>(true);
            if (helpView.DataContext is null)
            {
                helpView.DataContext = viewModelFactory.Create<HelpViewModel>();
            }
            helpView.Show();
            helpView.Activate();
        }

        private void OnClickedResetWindowIcon(object commandParam)
        {
            WindowState = WindowState.Normal;
            MainWindowWidth = MAIN_WINDOW_MIN_WIDTH;
            MainWindowHeight = MAIN_WINDOW_MIN_HEIGHT;
            SidebarWidth = SidebarMinWidth = SIDEBAR_MIN_WIDTH;
        }

        private void OnConfirmedTotalReset()
        {
            reset = true;
            Application.Current.Shutdown();
        }

        private async void OnRefreshAuth()
        {
            // If there is no current token,
            // instantly interrupt everything and prompt the user to log in.
            if (user.Token is null || string.IsNullOrEmpty(user.Token.Item2))
            {
                Logout();
                return;
            }

            var newToken = await userService.RefreshAuthToken(user.Id, user.Token.Item2);
            if (string.IsNullOrEmpty(newToken))
            {
                Logout();
                return;
            }

            user.Token = new Tuple<DateTime, string>(DateTime.UtcNow, newToken);
        }

        private void Logout()
        {
            user.Token = null;
            user.PasswordSHA512 = null;

            UIEnabled = false;
            ShowLoginControl();

            if (scheduledAuthRefresh.HasValue)
            {
                methodQ.Cancel(scheduledAuthRefresh.Value);
                scheduledAuthRefresh = null;
            }
        }
    }
}
