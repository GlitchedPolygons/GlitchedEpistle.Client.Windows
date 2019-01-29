﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Media.Imaging;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.UserControls;
using GlitchedPolygons.ExtensionMethods.RSAXmlPemStringConverter;

using ZXing;
using ZXing.Common;
using Prism.Events;
using ZXing.Rendering;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    // TODO: Update the progress bar's tooltip dynamically (showing the user how much time is remaining until account expiration).
    public class MainViewModel : ViewModel
    {
        #region Constants
        public const double SIDEBAR_MIN_WIDTH = 345;
        public const double SIDEBAR_MAX_WIDTH = 666;
        public const double MAIN_WINDOW_MIN_WIDTH = 800;
        public const double MAIN_WINDOW_MIN_HEIGHT = 480;

        // Injections:
        private readonly User user;
        private readonly ISettings settings;
        private readonly IUserService userService;
        private readonly IWindowFactory windowFactory;
        private readonly IEventAggregator eventAggregator;
        private readonly IViewModelFactory viewModelFactory;
        #endregion

        #region Commands
        public ICommand ClosedCommand { get; }
        public ICommand HelpButtonCommand { get; }
        public ICommand ResetWindowButtonCommand { get; }
        public ICommand SettingsButtonCommand { get; }
        public ICommand CreateConvoButtonCommand { get; }
        public ICommand ChangePasswordButtonCommand { get; }
        public ICommand ExportUserButtonCommand { get; }
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

        private HelpView helpView;
        private SettingsView settingsView;
        private UserExportView userExportView;

        public MainViewModel(ISettings settings, IEventAggregator eventAggregator, IUserService userService, IWindowFactory windowFactory, IViewModelFactory viewModelFactory, User user)
        {
            this.user = user;
            this.settings = settings;
            this.eventAggregator = eventAggregator;
            this.windowFactory = windowFactory;
            this.viewModelFactory = viewModelFactory;
            this.userService = userService;

            ClosedCommand = new DelegateCommand(OnClosed);
            SettingsButtonCommand = new DelegateCommand(OnClickedSettingsIcon);
            ResetWindowButtonCommand = new DelegateCommand(OnClickedResetWindowIcon);
            HelpButtonCommand = new DelegateCommand(OnClickedHelpIcon);
            CreateConvoButtonCommand = new DelegateCommand(OnClickedCreateConvo);
            ChangePasswordButtonCommand = new DelegateCommand(OnClickedChangePassword);
            ExportUserButtonCommand = new DelegateCommand(OnClickedExportUser);
            LogoutButtonCommand = new DelegateCommand(OnClickedLogout);

            // Update the username label on the main window when that setting has changed.
            eventAggregator.GetEvent<UsernameChangedEvent>().Subscribe(newUsername => Username = newUsername);

            // Update the main control when login was successful.
            eventAggregator.GetEvent<LoginSucceededEvent>().Subscribe(OnLoginSuccessful);

            // Show the 2FA secret QR code + list of backup codes after the registration form has been submitted successfully.
            eventAggregator.GetEvent<UserCreationSucceededEvent>().Subscribe(OnUserCreationSuccessful);

            // After a successful user creation, show the login screen.
            eventAggregator.GetEvent<UserCreationVerifiedEvent>().Subscribe(OnUserCreationVerified);

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

        private void OnClosed(object commandParam)
        {
            userExportView?.Close();
            settingsView?.Close();
            helpView?.Close();

            // Save the window's state before termination.
            settings.Load();
            var c = CultureInfo.InvariantCulture;
            settings[nameof(WindowState)] = WindowState.ToString();
            settings[nameof(MainWindowWidth)] = ((int)MainWindowWidth).ToString(c);
            settings[nameof(MainWindowHeight)] = ((int)MainWindowHeight).ToString(c);
            settings[nameof(SidebarWidth)] = ((int)SidebarWidth).ToString(c);
            settings.Save();
        }

        private void OnLoginSuccessful()
        {
            MainControl = null;
            UIEnabled = true;
            settings.Load();
            Username = settings[nameof(Username), SettingsViewModel.DEFAULT_USERNAME];
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

        }

        private void OnClickedChangePassword(object commandParam)
        {

        }

        private void OnClickedExportUser(object commandParam)
        {
            userExportView = windowFactory.Create<UserExportView>(true);

            // When opening views that only exist one at a time,
            // it's important not to recreate the viewmodel every time,
            // as that would override any changes made.
            // Therefore, check if the view already has a data context that isn't null.
            if (userExportView.DataContext is null)
            {
                userExportView.DataContext = viewModelFactory.Create<UserExportViewModel>();
            }

            userExportView.Show();
            userExportView.Activate();
        }

        private void OnClickedLogout(object commandParam)
        {
            if (MainControl is LoginView)
            {
                return;
            }

            user.Token = null;
            user.PasswordSHA512 = null;

            UIEnabled = false;
            ShowLoginControl();
        }

        private void OnClickedHelpIcon(object commandParam)
        {
            helpView = windowFactory.Create<HelpView>(true);
            if (helpView.DataContext is null)
            {
                helpView.DataContext = viewModelFactory.Create<HelpViewModel>();
            }
            helpView.Show();
            helpView.Activate();
        }

        private void OnClickedSettingsIcon(object commandParam)
        {
            settingsView = windowFactory.Create<SettingsView>(true);
            if (settingsView.DataContext is null)
            {
                settingsView.DataContext = viewModelFactory.Create<SettingsViewModel>();
            }
            settingsView.Show();
            settingsView.Activate();
        }

        private void OnClickedResetWindowIcon(object commandParam)
        {
            WindowState = WindowState.Normal;
            MainWindowWidth = MAIN_WINDOW_MIN_WIDTH;
            MainWindowHeight = MAIN_WINDOW_MIN_HEIGHT;
            SidebarWidth = SidebarMinWidth = SIDEBAR_MIN_WIDTH;
        }
    }
}
