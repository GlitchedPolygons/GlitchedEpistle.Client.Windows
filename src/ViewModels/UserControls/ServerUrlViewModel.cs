using System.Windows.Input;

using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.ServerHealth;
using GlitchedPolygons.GlitchedEpistle.Client.Utilities;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class ServerUrlViewModel : ViewModel
    {
        private readonly IServerConnectionTest test;

        #region Commands
        public ICommand ConnectCommand { get; }
        #endregion

        #region UI Bindings
        private string serverUrl = "https://epistle.glitchedpolygons.com";
        public string ServerUrl { get => serverUrl; set => Set(ref serverUrl, value); }
        #endregion

        public ServerUrlViewModel(IServerConnectionTest test)
        {
            this.test = test;
            ConnectCommand = new DelegateCommand(OnClickedConnect);
        }

        private async void OnClickedConnect(object commandParam)
        {
            UrlUtility.SetEpistleServerUrl(ServerUrl);
            bool success = await test.TestConnection();

            // TODO: handle both success and failure here
        }
    }
}
