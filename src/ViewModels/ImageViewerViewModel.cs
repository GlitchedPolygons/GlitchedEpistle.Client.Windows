/*
    Glitched Epistle - Windows Client
    Copyright (C) 2020 Raphael Beck

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.IO;

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

        private MemoryStream imageBytesStream;
        public MemoryStream ImageBytesStream
        {
            get
            {
                if (imageBytesStream is null)
                {
                    imageBytesStream = new MemoryStream(ImageBytes ?? new byte[0]);
                }
                return imageBytesStream;
            }
        }
        #endregion
    }
}
