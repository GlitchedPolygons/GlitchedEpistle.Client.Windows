using System.Diagnostics;
using System.Windows.Input;

using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class ExpiredViewModel : ViewModel
    {
        #region Commands
        public ICommand RedeemButtonCommand { get; }
        public ICommand BuyButtonCommand { get; }
        #endregion

        public ExpiredViewModel(IWindowFactory windowFactory, IViewModelFactory viewModelFactory)
        {
            BuyButtonCommand = new DelegateCommand(_ => Process.Start("https://www.glitchedpolygons.com/extend-epistle-sub"));
            RedeemButtonCommand = new DelegateCommand(_ =>
            {
                var settingsView = windowFactory.Create<SettingsView>(true);
                if (settingsView.DataContext is null)
                {
                    settingsView.DataContext = viewModelFactory.Create<SettingsViewModel>();
                }
                settingsView.Show();
                settingsView.Activate();
            });
        }
    }
}
