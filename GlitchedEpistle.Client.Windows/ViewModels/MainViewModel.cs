using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Globalization;

using GlitchedPolygons.Services.MethodQ;
using GlitchedPolygons.ExtensionMethods.RSAXmlPemStringConverter;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.UserControls;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;

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
        public const double MAIN_WINDOW_MIN_HEIGHT = 530;
        private const int CLIPBOARD_COPY_CONFIRMATION_TICK_DURATION = 3;
        private static readonly TimeSpan AUTH_REFRESH_INTERVAL = TimeSpan.FromMinutes(15);

        // Injections:
        private readonly User user;
        private readonly ILogger logger;
        private readonly IMethodQ methodQ;
        private readonly ISettings settings;
        private readonly IUserService userService;
        private readonly IConvoProvider convoProvider;
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
        public ICommand JoinConvoButtonCommand { get; }
        public ICommand ChangePasswordButtonCommand { get; }
        public ICommand LogoutButtonCommand { get; }
        public ICommand CopyUserIdToClipboardCommand { get; }
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

        private Visibility clipboardTickVisibility = Visibility.Hidden;
        public Visibility ClipboardTickVisibility { get => clipboardTickVisibility; set => Set(ref clipboardTickVisibility, value); }

        private Control mainControl;
        public Control MainControl { get => mainControl; set => Set(ref mainControl, value); }

        private Control convosListControl;
        public Control ConvosListControl { get => convosListControl; set => Set(ref convosListControl, value); }
        #endregion

        private bool reset = false;
        private ulong? scheduledAuthRefresh = null, scheduledExpirationDialog = null, scheduledHideGreenTickIcon = null;

        public MainViewModel(ISettings settings, IEventAggregator eventAggregator, IUserService userService, IWindowFactory windowFactory, IViewModelFactory viewModelFactory, User user, IMethodQ methodQ, ILogger logger, IConvoProvider convoProvider)
        {
            this.user = user;
            this.logger = logger;
            this.methodQ = methodQ;
            this.settings = settings;
            this.userService = userService;
            this.convoProvider = convoProvider;
            this.windowFactory = windowFactory;
            this.viewModelFactory = viewModelFactory;
            this.eventAggregator = eventAggregator;

            #region Button click commands

            ResetWindowButtonCommand = new DelegateCommand(_ =>
            {
                WindowState = WindowState.Normal;
                MainWindowWidth = MAIN_WINDOW_MIN_WIDTH;
                MainWindowHeight = MAIN_WINDOW_MIN_HEIGHT;
                SidebarWidth = SidebarMinWidth = SIDEBAR_MIN_WIDTH;
            });

            SettingsButtonCommand = new DelegateCommand(_ => windowFactory.OpenWindow<SettingsView, SettingsViewModel>(true, true));
            HelpButtonCommand = new DelegateCommand(_ => windowFactory.OpenWindow<HelpView, HelpViewModel>(false, true));
            CreateConvoButtonCommand = new DelegateCommand(_ => windowFactory.OpenWindow<CreateConvoView, CreateConvoViewModel>(true, true));
            JoinConvoButtonCommand = new DelegateCommand(_ => windowFactory.OpenWindow<JoinConvoDialogView, JoinConvoDialogViewModel>(true, true));
            ChangePasswordButtonCommand = new DelegateCommand(_ => windowFactory.OpenWindow<ChangePasswordView, ChangePasswordViewModel>(true, true));
            LogoutButtonCommand = new DelegateCommand(_ => Logout());
            CopyUserIdToClipboardCommand = new DelegateCommand(_ =>
            {
                Clipboard.SetText(UserId);
                ClipboardTickVisibility = Visibility.Visible;

                if (scheduledHideGreenTickIcon.HasValue)
                    methodQ.Cancel(scheduledHideGreenTickIcon.Value);

                scheduledHideGreenTickIcon = methodQ.Schedule(() =>
                {
                    ClipboardTickVisibility = Visibility.Hidden;
                    scheduledHideGreenTickIcon = null;
                }, DateTime.UtcNow.AddSeconds(CLIPBOARD_COPY_CONFIRMATION_TICK_DURATION));
            });

            #endregion

            ClosedCommand = new DelegateCommand(OnClosed);

            // Update the username label on the main window when that setting has changed.
            eventAggregator.GetEvent<UsernameChangedEvent>().Subscribe(newUsername => Username = newUsername);

            // Update the main control when login was successful.
            eventAggregator.GetEvent<LoginSucceededEvent>().Subscribe(OnLoginSuccessful);

            // Show the 2FA secret QR code + list of backup codes after the registration form has been submitted successfully.
            eventAggregator.GetEvent<UserCreationSucceededEvent>().Subscribe(OnUserCreationSuccessful);

            // After a successful user creation, show the login screen.
            eventAggregator.GetEvent<UserCreationVerifiedEvent>().Subscribe(ShowLoginControl);

            // If the user agreed to delete all of his data on the local machine, respect his will
            // and get rid of everything (even preventing new settings to be written out on app shutdown too).
            eventAggregator.GetEvent<ResetConfirmedEvent>().Subscribe(() => { reset = true; Application.Current.Shutdown(); });

            // When the user redeemed a coupon, update the account's remaining time bar in main menu.
            eventAggregator.GetEvent<CouponRedeemedEvent>().Subscribe(OnCouponRedeemedSuccessfully);

            eventAggregator.GetEvent<JoinedConvoEvent>().Subscribe(OnJoinedConvo);

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
            }

            if (string.IsNullOrEmpty(UserId))
            {
                ShowRegisterControl();
            }
            else
            {
                ShowLoginControl();
            }

            ConvosListControl = new ConvosListView { DataContext = viewModelFactory.Create<ConvosListViewModel>() };
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

        private void OnClosed(object commandParam)
        {
            // Don't save out anything and delete
            // epistle root directory if the app's been reset.
            if (reset)
            {
                var dir = new DirectoryInfo(Paths.ROOT_DIRECTORY);
                if (dir.Exists)
                {
                    dir.DeleteRecursively();
                }
                Application.Current.Shutdown();
            }
            else
            {
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
        }

        private async void UpdateUserExp()
        {
            // Gather user expiration UTC from server.
            DateTime? exp = await userService.GetUserExpirationUTC(user.Id);
            if (!exp.HasValue)
            {
                return;
            }

            // Update the UI accordingly (blue progress bar + tooltip).
            user.ExpirationUTC = exp.Value;
            DateTime utcNow = DateTime.UtcNow;
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
                TimeSpan remainingTime = user.ExpirationUTC - utcNow;
                if (remainingTime < TimeSpan.FromDays(5))
                {
                    ShowExpirationReminderControl();

                    if (scheduledExpirationDialog.HasValue)
                    {
                        methodQ.Cancel(scheduledExpirationDialog.Value);
                    }

                    scheduledExpirationDialog = methodQ.Schedule(ShowExpiredControl, user.ExpirationUTC);
                }
            }

            // When the user account is expired,
            // the UI should be inactive (and inoperable).
            UIEnabled = !expired;
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

            // Load the user's RSA keys into the User instance.
            if (!File.Exists(Paths.PUBLIC_KEY_PATH))
            {
                logger.LogError("Login was successful but no public key was found on disk! Did you delete/modify that key file manually?");
                throw new FileNotFoundException("No public key found on disk!");
            }

            user.PublicKeyXml = File.ReadAllText(Paths.PUBLIC_KEY_PATH).PemToXml();
            user.PublicKey = RSAParametersExtensions.FromXml(user.PublicKeyXml);

            if (!File.Exists(Paths.PRIVATE_KEY_PATH))
            {
                logger.LogError("Login was successful but no private key was found on disk! Did you delete/modify that key file manually?");
                throw new FileNotFoundException("No private key found on disk!");
            }

            user.PrivateKey = RSAParametersExtensions.FromXml(File.ReadAllText(Paths.PRIVATE_KEY_PATH).PemToXml());
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
            viewModel.Secret = userCreationResponse.TotpSecret;
            viewModel.QR = qrWriter.Write($"otpauth://totp/GlitchedEpistle:{userCreationResponse.Id}?secret={userCreationResponse.TotpSecret}");
            viewModel.BackupCodes = userCreationResponse.TotpEmergencyBackupCodes;

            MainControl = new UserCreationSuccessfulView { DataContext = viewModel };
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

        private void OnJoinedConvo(Convo convo)
        {
            var viewModel = viewModelFactory.Create<ActiveConvoViewModel>();
            viewModel.ActiveConvo = convo;
            MainControl = new ActiveConvoView { DataContext = viewModel };
        }

        private void Logout()
        {
            if (MainControl is LoginView)
                return;

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
