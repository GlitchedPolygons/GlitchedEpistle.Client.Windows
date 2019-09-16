using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class UsernamePromptViewModel : ViewModel
    {
        #region Constants
        // Injections:
        private readonly IUserSettings userSettings;
        private readonly IEventAggregator eventAggregator;
        #endregion

        #region Commands
        public ICommand AcceptCommand { get; }
        #endregion

        #region UI Bindings
        private string username = string.Empty;
        public string Username { get => username; set => Set(ref username, value); }

        private string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; set => Set(ref errorMessage, value); }

        private bool uiEnabled = true;
        public bool UIEnabled { get => uiEnabled; set => Set(ref uiEnabled, value); }
        #endregion

        public UsernamePromptViewModel(IUserSettings userSettings, IEventAggregator eventAggregator)
        {
            this.userSettings = userSettings;
            this.eventAggregator = eventAggregator;

            AcceptCommand = new DelegateCommand(OnAccept);
        }

        private void OnAccept(object commandParam)
        {
            if (Username.NullOrEmpty())
            {
                return;
            }

            userSettings.Username = Username;
            userSettings.Save();

            eventAggregator.GetEvent<UsernameChangedEvent>().Publish(userSettings.Username);
            eventAggregator.GetEvent<ClearMainControlEvent>().Publish();
        }
    }
}
