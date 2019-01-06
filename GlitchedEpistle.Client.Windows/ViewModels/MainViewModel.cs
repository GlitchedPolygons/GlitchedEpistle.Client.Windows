using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

using Prism.Events;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class MainViewModel : ViewModel
    {
        #region Constants
        public const double SIDEBAR_MIN_WIDTH = 340;
        public const double MAIN_WINDOW_MIN_WIDTH = 800;
        public const double MAIN_WINDOW_MIN_HEIGHT = 450;
        private readonly App app = Application.Current as App;

        // Injections:
        private readonly ISettings settings;
        private readonly IWindowFactory windowFactory;
        private readonly IEventAggregator eventAggregator;
        #endregion

        private SettingsView settingsView;//TODO: this is bad!
        
        #region Commands
        public ICommand LoadedCommand { get; }
        public ICommand ClosedCommand { get; }
        public ICommand SettingsButtonCommand { get; }
        public ICommand CreateConvoButtonCommand { get; }
        public ICommand ChangePasswordButtonCommand { get; }
        public ICommand ExportUserButtonCommand { get; }
        public ICommand LogoutButtonCommand { get; }
        #endregion

        #region UI Bindings
        private string usernameLabel = SettingsViewModel.DEFAULT_USERNAME;
        public string UsernameLabel { get => usernameLabel; set => Set(ref usernameLabel, value); }

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
        #endregion

        public MainViewModel(ISettings settings, IEventAggregator eventAggregator, IWindowFactory windowFactory)
        {
            this.settings = settings;
            this.eventAggregator = eventAggregator;
            this.windowFactory = windowFactory;

            ClosedCommand = new DelegateCommand(OnClosed);
            SettingsButtonCommand = new DelegateCommand(OnClickedSettingsIcon);
            CreateConvoButtonCommand = new DelegateCommand(OnClickedCreateConvo);
            ChangePasswordButtonCommand = new DelegateCommand(OnClickedChangePassword);
            ExportUserButtonCommand = new DelegateCommand(OnClickedExportUser);
            LogoutButtonCommand = new DelegateCommand(OnClickedLogout);

            // Load up the settings on startup.
            if (settings.Load())
            {
                UsernameLabel = settings[nameof(SettingsViewModel.Username), SettingsViewModel.DEFAULT_USERNAME];

                Enum.TryParse<WindowState>(settings[nameof(WindowState), WindowState.Normal.ToString()], out var windowState);
                WindowState = windowState;

                MainWindowWidth = Math.Abs(settings[nameof(MainWindowWidth), MAIN_WINDOW_MIN_WIDTH]);
                MainWindowHeight = Math.Abs(settings[nameof(MainWindowHeight), MAIN_WINDOW_MIN_HEIGHT]);

                double w = Math.Abs(settings[nameof(SidebarWidth), SIDEBAR_MIN_WIDTH]);
                SidebarWidth = w < SIDEBAR_MIN_WIDTH ? SIDEBAR_MIN_WIDTH : w > MainWindowWidth ? MainWindowWidth : w;
            }

            // Update the username label on the main window when that setting has changed.
            eventAggregator.GetEvent<UsernameChangedEvent>().Subscribe(newUsername => UsernameLabel = newUsername);
        }

        private void OnClosed(object commandParam)
        {
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

        }

        private void OnClickedLogout(object commandParam)
        {

        }

        private void OnClickedSettingsIcon(object commandParam)
        {
            settingsView = windowFactory.GetWindow<SettingsView>(true);
            settingsView.DataContext = new SettingsViewModel(settings, eventAggregator);
            settingsView.Show();
            settingsView.Activate();
        }
    }
}
