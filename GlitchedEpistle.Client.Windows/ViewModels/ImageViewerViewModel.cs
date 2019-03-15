using System;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class ImageViewerViewModel : ViewModel
    {
        #region Constants
        // Injections:
        #endregion

        #region Commands

        #endregion

        #region UI Bindings
        private byte[] imageBytes;
        public byte[] ImageBytes { get => imageBytes; set => Set(ref imageBytes, value); }
        #endregion

        public ImageViewerViewModel()
        {
            
        }
    }
}
