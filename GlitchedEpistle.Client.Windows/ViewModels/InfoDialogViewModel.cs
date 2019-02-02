using System;
using System.Windows.Input;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class InfoDialogViewModel : ViewModel, ICloseable
    {
        #region Constants

        #endregion

        #region Events
        public event EventHandler<EventArgs> RequestedClose;
        #endregion

        #region UI Bindings
        private string title = "Information";
        public string Title { get => title; set => Set(ref title, value); }

        private string text = string.Empty;
        public string Text { get => text; set => Set(ref text, value); }

        private string okButtonText = "Okay";
        public string OkButtonText { get => okButtonText; set => Set(ref okButtonText, value); }
        #endregion

        #region Commands
        public ICommand OkButtonCommand { get; }
        #endregion

        public InfoDialogViewModel()
        {
            OkButtonCommand = new DelegateCommand(OnClickedOkay);
        }

        private void OnClickedOkay(object commandParam)
        {
            RequestedClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
