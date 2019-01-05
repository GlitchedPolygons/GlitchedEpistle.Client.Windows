using System;
using System.Windows;
using System.Windows.Input;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class MainViewModel : ViewModel
    {
        #region Constants
        private const double SIDEBAR_MIN_WIDTH = 340;
        private const double MAIN_WINDOW_MIN_WIDTH = 800;
        private const double MAIN_WINDOW_MIN_HEIGHT = 450;
        private readonly ISettings settings;
        private readonly App app = Application.Current as App;
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

        private double leftColumnWidth;
        public double LeftColumnWidth { get => leftColumnWidth; set => Set(ref leftColumnWidth, value); }
        #endregion

        public MainViewModel(ISettings settings)
        {
            this.settings = settings;

            LoadedCommand = new DelegateCommand(OnLoaded);
            ClosedCommand = new DelegateCommand(OnClosed);
            SettingsButtonCommand = new DelegateCommand(OnClickedSettingsIcon);
            CreateConvoButtonCommand = new DelegateCommand(OnClickedCreateConvo);
            ChangePasswordButtonCommand = new DelegateCommand(OnClickedChangePassword);
            ExportUserButtonCommand = new DelegateCommand(OnClickedExportUser);
            LogoutButtonCommand = new DelegateCommand(OnClickedLogout);
        }

        private void OnLoaded(object commandParam)
        {

        }

        private void OnClosed(object commandParam)
        {
            settingsView?.Close();
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
            settingsView = app.GetWindow<SettingsView>(true);
            settingsView.DataContext = new SettingsViewModel(settings);
            settingsView.Closed += SettingsView_Closed;
            settingsView.Show();
            settingsView.Activate();
        }

        private void SettingsView_Closed(object sender, EventArgs e)
        {
            if (settings.Load())
            {
                UsernameLabel = settings[nameof(SettingsViewModel.Username), SettingsViewModel.DEFAULT_USERNAME];
            }
            settingsView = null;
        }
    }
}
