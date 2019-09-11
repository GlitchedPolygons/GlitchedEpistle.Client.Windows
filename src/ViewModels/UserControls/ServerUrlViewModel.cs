using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.ServerHealth;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class ServerUrlViewModel : ViewModel
    {
        private readonly IServerConnectionTest test;

        public ServerUrlViewModel(IServerConnectionTest test)
        {
            this.test = test;
        }
    }
}
