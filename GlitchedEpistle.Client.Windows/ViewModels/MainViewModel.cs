using System;
using System.Windows;
using System.Windows.Input;
using System.Globalization;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using Prism.Events;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    // TODO: Update the progress bar's tooltip dynamically (showing the user how much time is remaining until account expiration).
    public class MainViewModel : ViewModel
    {
        #region Constants
        public const double SIDEBAR_MIN_WIDTH = 345;
        public const double MAIN_WINDOW_MIN_WIDTH = 800;
        public const double MAIN_WINDOW_MIN_HEIGHT = 450;

        // Injections:
        private readonly ISettings settings;
        private readonly IEventAggregator eventAggregator;
        private readonly IWindowFactory windowFactory;
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
        private string username = SettingsViewModel.DEFAULT_USERNAME;
        public string Username { get => username; set => Set(ref username, value); }

        private string userId = string.Empty;
        public string UserId { get => userId; set => Set(ref userId, value); }

        private double sidebarWidth = SIDEBAR_MIN_WIDTH;
        public double SidebarWidth { get => sidebarWidth; set => Set(ref sidebarWidth, value); }

        private double sidebarMinWidth = SIDEBAR_MIN_WIDTH;
        public double SidebarMinWidth { get => sidebarMinWidth; set => Set(ref sidebarMinWidth, value); }

        private double mainWindowWidth = MAIN_WINDOW_MIN_WIDTH;
        public double MainWindowWidth { get => mainWindowWidth; set => Set(ref mainWindowWidth, value); }

        private double mainWindowHeight = MAIN_WINDOW_MIN_HEIGHT;
        public double MainWindowHeight { get => mainWindowHeight; set => Set(ref mainWindowHeight, value); }

        private WindowState windowState = WindowState.Normal;
        public WindowState WindowState { get => windowState; set => Set(ref windowState, value); }

        private string progressBarTooltip = "Subscription expires the 24th of December, 2018 at 15:30. Click to extend now!";
        public string ProgressBarTooltip { get => progressBarTooltip; set => Set(ref progressBarTooltip, value); }

        private double progressBarValue = 75;
        public double ProgressBarValue { get => progressBarValue; set => Set(ref progressBarValue, value); }
        #endregion

        private HelpView helpView;
        private SettingsView settingsView;
        private UserExportView userExportView;

        public MainViewModel(ISettings settings, IEventAggregator eventAggregator, IWindowFactory windowFactory, IViewModelFactory viewModelFactory)
        {
            this.settings = settings;
            this.eventAggregator = eventAggregator;
            this.windowFactory = windowFactory;
            this.viewModelFactory = viewModelFactory;

            ClosedCommand = new DelegateCommand(OnClosed);
            SettingsButtonCommand = new DelegateCommand(OnClickedSettingsIcon);
            ResetWindowButtonCommand = new DelegateCommand(OnClickedResetWindowIcon);
            HelpButtonCommand = new DelegateCommand(OnClickedHelpIcon);
            CreateConvoButtonCommand = new DelegateCommand(OnClickedCreateConvo);
            ChangePasswordButtonCommand = new DelegateCommand(OnClickedChangePassword);
            ExportUserButtonCommand = new DelegateCommand(OnClickedExportUser);
            LogoutButtonCommand = new DelegateCommand(OnClickedLogout);

            // Load up the settings on startup.
            if (settings.Load())
            {
                Username = settings[nameof(SettingsViewModel.Username), SettingsViewModel.DEFAULT_USERNAME];

                Enum.TryParse<WindowState>(settings[nameof(WindowState), WindowState.Normal.ToString()], out var loadedWindowState);
                WindowState = loadedWindowState;

                MainWindowWidth = Math.Abs(settings[nameof(MainWindowWidth), MAIN_WINDOW_MIN_WIDTH]);
                MainWindowHeight = Math.Abs(settings[nameof(MainWindowHeight), MAIN_WINDOW_MIN_HEIGHT]);

                double w = Math.Abs(settings[nameof(SidebarWidth), SIDEBAR_MIN_WIDTH]);
                SidebarWidth = w < SIDEBAR_MIN_WIDTH ? SIDEBAR_MIN_WIDTH : w > MainWindowWidth ? SIDEBAR_MIN_WIDTH : w;
            }

            string userId = settings[nameof(UserId)];
            if (!string.IsNullOrEmpty(userId))
            {

            }

            // Update the username label on the main window when that setting has changed.
            eventAggregator.GetEvent<UsernameChangedEvent>().Subscribe(newUsername => Username = newUsername);
        }

        private void OnClosed(object commandParam)
        {
            userExportView?.Close();
            settingsView?.Close();

            settings.Load();
            var c = CultureInfo.InvariantCulture;
            settings[nameof(WindowState)] = WindowState.ToString();
            settings[nameof(MainWindowWidth)] = ((int)MainWindowWidth).ToString(c);
            settings[nameof(MainWindowHeight)] = ((int)MainWindowHeight).ToString(c);
            settings[nameof(SidebarWidth)] = ((int)SidebarWidth).ToString(c);
            settings.Save();
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
