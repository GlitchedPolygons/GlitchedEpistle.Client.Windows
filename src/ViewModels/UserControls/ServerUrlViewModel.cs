using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.GlitchedEpistle.Client.Utilities;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.ServerHealth;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Localization;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class ServerUrlViewModel : ViewModel
    {
        private readonly IAppSettings appSettings;
        private readonly IServerConnectionTest test;
        private readonly ILocalization localization;
        private readonly IEventAggregator eventAggregator;

        #region Commands
        public ICommand ConnectCommand { get; }
        public ICommand TestConnectionCommand { get; }
        #endregion

        #region UI Bindings
        private string serverUrl = "https://epistle.glitchedpolygons.com";
        public string ServerUrl
        {
            get => serverUrl;
            set
            {
                Set(ref serverUrl, value);
                ConnectionOk = false;
            }
        }

        private bool uiEnabled = true;
        public bool UIEnabled { get => uiEnabled; set => Set(ref uiEnabled, value); }

        private bool connectionOk = false;
        public bool ConnectionOk { get => connectionOk; set => Set(ref connectionOk, value); }

        private Visibility clipboardTickVisibility = Visibility.Hidden;
        public Visibility ClipboardTickVisibility { get => clipboardTickVisibility; set => Set(ref clipboardTickVisibility, value); }

        private Visibility testingLabelVisibility = Visibility.Hidden;
        public Visibility TestingLabelVisibility { get => testingLabelVisibility; set => Set(ref testingLabelVisibility, value); }
        #endregion

        public ServerUrlViewModel(IServerConnectionTest test, IAppSettings appSettings, IEventAggregator eventAggregator, ILocalization localization)
        {
            this.test = test;
            this.appSettings = appSettings;
            this.localization = localization;
            this.eventAggregator = eventAggregator;

            string url = appSettings.ServerUrl;
            if (url.NotNullNotEmpty())
            {
                ServerUrl = url;
            }

            ConnectCommand = new DelegateCommand(OnClickedConnect);
            TestConnectionCommand = new DelegateCommand(OnClickedTestConnection);
        }

        private void OnClickedTestConnection(object commandParam)
        {
            UIEnabled = false;
            ClipboardTickVisibility = Visibility.Hidden;
            TestingLabelVisibility = Visibility.Visible;

            Task.Run(async () =>
            {
                ConnectionOk = await test.TestConnection(ServerUrl);
                if (ConnectionOk)
                {
                    UrlUtility.SetEpistleServerUrl(ServerUrl);
                }
                TestingLabelVisibility = Visibility.Hidden;
                ClipboardTickVisibility = ConnectionOk ? Visibility.Visible : Visibility.Hidden;
                ErrorMessage = ConnectionOk ? null : localization["ConnectionToTheSpecifiedEpistleServerFailed"];
                UIEnabled = true;
            });
        }

        private void OnClickedConnect(object commandParam)
        {
            UIEnabled = false;
            Task.Run(async () =>
            {
                bool success = await test.TestConnection();
                if (success)
                {
                    UrlUtility.SetEpistleServerUrl(ServerUrl);
                    appSettings.ServerUrl = UrlUtility.EpistleBaseUrl;
                    ExecUI(() => eventAggregator.GetEvent<LogoutEvent>().Publish());
                    return;
                }
                ErrorMessage = localization["ConnectionToTheSpecifiedEpistleServerFailed"];
            });
        }
    }
}
