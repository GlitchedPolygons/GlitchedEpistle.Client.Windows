using System.Windows.Input;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class ExpirationReminderViewModel : ViewModel
    {
        #region Constants
        // Injections:
        private readonly User user;
        private readonly IWindowFactory windowFactory;
        private readonly IViewModelFactory viewModelFactory;
        #endregion

        #region Commands
        public ICommand ExtendButtonCommand { get; }
        #endregion

        #region UI Bindings
        private string reminderText;
        public string ReminderText { get => reminderText; set => Set(ref reminderText, value); }
        #endregion

        public ExpirationReminderViewModel(User user, IWindowFactory windowFactory, IViewModelFactory viewModelFactory)
        {
            this.user = user;
            this.windowFactory = windowFactory;
            this.viewModelFactory = viewModelFactory;

            ExtendButtonCommand = new DelegateCommand(OnClickedExtend);

            ReminderText = "";
        }

        private void OnClickedExtend(object commandParam)
        {
            var settingsView = windowFactory.Create<SettingsView>(true);
            if (settingsView.DataContext is null)
            {
                settingsView.DataContext = viewModelFactory.Create<SettingsViewModel>();
            }
            settingsView.Show();
            settingsView.Activate();
        }
    }
}
