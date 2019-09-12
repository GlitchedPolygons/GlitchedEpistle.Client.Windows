using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.ServerHealth;
using GlitchedPolygons.GlitchedEpistle.Client.Utilities;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class ServerUrlViewModel : ViewModel
    {
        private readonly ISettings settings;
        private readonly IServerConnectionTest test;
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

        private string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; set => Set(ref errorMessage, value); }

        private bool uiEnabled = true;
        public bool UIEnabled { get => uiEnabled; set => Set(ref uiEnabled, value); }

        private bool connectionOk = false;
        public bool ConnectionOk { get => connectionOk; set => Set(ref connectionOk, value); }

        private Visibility clipboardTickVisibility = Visibility.Hidden;
        public Visibility ClipboardTickVisibility { get => clipboardTickVisibility; set => Set(ref clipboardTickVisibility, value); }

        private Visibility testingLabelVisibility = Visibility.Hidden;
        public Visibility TestingLabelVisibility { get => testingLabelVisibility; set => Set(ref testingLabelVisibility, value); }
        #endregion

        public ServerUrlViewModel(IServerConnectionTest test, ISettings settings, IEventAggregator eventAggregator)
        {
            this.test = test;
            this.settings = settings;
            this.eventAggregator = eventAggregator;

            string url = settings["ServerUrl"];
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
                bool success = await test.TestConnection(ServerUrl);
                if (success) UrlUtility.SetEpistleServerUrl(ServerUrl);
                ExecUI(() =>
                {
                    ConnectionOk = success;
                    TestingLabelVisibility = Visibility.Hidden;
                    ClipboardTickVisibility = ConnectionOk ? Visibility.Visible : Visibility.Hidden;
                    ErrorMessage = ConnectionOk ? null : "The connection to the specified Epistle server could not be established.";
                    UIEnabled = true;
                });
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
                    settings["ServerUrl"] = UrlUtility.EpistleBaseUrl;
                    settings.Save();
                    ExecUI(() => eventAggregator.GetEvent<LogoutEvent>().Publish());
                    return;
                }
                ExecUI(() => ErrorMessage = "The connection to the specified Epistle server could not be established.");
            });
        }
    }
}
