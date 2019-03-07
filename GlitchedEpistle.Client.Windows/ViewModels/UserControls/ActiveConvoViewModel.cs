using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.Services.MethodQ;
using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class ActiveConvoViewModel : ViewModel
    {
        #region Constants
        // Injections:
        private readonly User user;
        private readonly IMethodQ methodQ;
        private readonly IConvoService convoService;
        private readonly IConvoProvider convoProvider;
        private readonly IEventAggregator eventAggregator;
        #endregion

        #region Commands
        public ICommand SendCommand { get; }
        public ICommand AttachCommand { get; }
        public ICommand CopyConvoIdToClipboardCommand { get; }
        #endregion

        #region UI Bindings
        private Visibility clipboardTickVisibility = Visibility.Hidden;
        public Visibility ClipboardTickVisibility { get => clipboardTickVisibility; set => Set(ref clipboardTickVisibility, value); }
        #endregion

        public Convo ActiveConvo { get; set; }

        private ulong? scheduledHideGreenTickIcon = null;

        public ActiveConvoViewModel(User user, IConvoService convoService, IConvoProvider convoProvider, IEventAggregator eventAggregator, IMethodQ methodQ)
        {
            this.user = user;
            this.methodQ = methodQ;
            this.convoService = convoService;
            this.convoProvider = convoProvider;
            this.eventAggregator = eventAggregator;

            SendCommand = new DelegateCommand(OnSend);
            AttachCommand = new DelegateCommand(OnClickedAttach);
            CopyConvoIdToClipboardCommand = new DelegateCommand(OnClickedCopyConvoIdToClipboard);
        }

        private void OnSend(object commandParam)
        {
            string text = commandParam as string;
            if (string.IsNullOrEmpty(text)) return;

            // TODO: Encrypt text here and prepare message body
            throw new NotImplementedException();
        }

        private void OnClickedAttach(object commandParam)
        {
            // TODO: Open file dialog here and then encode, encrypt and package message and prepare for submission
            throw new NotImplementedException();
        }

        private void OnClickedCopyConvoIdToClipboard(object commandParam)
        {
            Clipboard.SetText(ActiveConvo.Id);
            ClipboardTickVisibility = Visibility.Visible;

            if (scheduledHideGreenTickIcon.HasValue)
                methodQ.Cancel(scheduledHideGreenTickIcon.Value);

            scheduledHideGreenTickIcon = methodQ.Schedule(() =>
            {
                ClipboardTickVisibility = Visibility.Hidden;
                scheduledHideGreenTickIcon = null;
            }, DateTime.UtcNow.AddSeconds(3));
        }
    }
}
