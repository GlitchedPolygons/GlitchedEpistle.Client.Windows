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
        private readonly IConvoService convoService;
        private readonly IConvoProvider convoProvider;
        private readonly IEventAggregator eventAggregator;
        #endregion

        #region Commands
        public ICommand CopyConvoIdToClipboardCommand { get; set; }
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
            this.convoService = convoService;
            this.convoProvider = convoProvider;
            this.eventAggregator = eventAggregator;

            CopyConvoIdToClipboardCommand = new DelegateCommand(_ =>
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
            });
        }
    }
}
