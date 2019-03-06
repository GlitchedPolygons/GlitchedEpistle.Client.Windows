using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
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

        public Convo ActiveConvo { get; set; }

        public ActiveConvoViewModel(User user, IConvoService convoService, IConvoProvider convoProvider, IEventAggregator eventAggregator)
        {
            this.user = user;
            this.convoService = convoService;
            this.convoProvider = convoProvider;
            this.eventAggregator = eventAggregator;
        }
    }
}
