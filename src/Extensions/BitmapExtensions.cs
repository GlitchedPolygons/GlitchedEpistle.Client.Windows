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

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Extensions
{
    /// <summary>
    /// Class that holds extension methods for <see cref="Bitmap"/>s.
    /// </summary>
    public static class BitmapExtensions
    {
        /// <summary>
        /// Converts a <see cref="Bitmap"/> to <see cref="BitmapSource"/>.
        /// </summary>
        /// <param name="bitmap">The <see cref="Bitmap"/> to convert to <see cref="BitmapSource"/>.</param>
        /// <returns>Converted <see cref="BitmapSource"/>.</returns>
        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            if (Application.Current.Dispatcher == null)
            {
                return null; // Is this even possible?
            }

            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // You need to specify the image format to fill the stream. Let's hope this is always PNG ;D
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // Make sure to create the bitmap in the UI thread...
                    if (InvokeRequired)
                    {
                        return (BitmapSource)Application.Current?.Dispatcher?.Invoke(
                            method: new Func<Stream, BitmapSource>(CreateBitmapSourceFromBitmap),
                            priority: DispatcherPriority.Normal,
                            arg: memoryStream);
                    }

                    return CreateBitmapSourceFromBitmap(memoryStream);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool InvokeRequired => Dispatcher.CurrentDispatcher != Application.Current?.Dispatcher;

        private static BitmapSource CreateBitmapSourceFromBitmap(Stream stream)
        {
            var bitmapDecoder = BitmapDecoder.Create(
                stream,
                cacheOption: BitmapCacheOption.OnLoad,
                createOptions: BitmapCreateOptions.PreservePixelFormat
            );

            // This will disconnect the stream from the image completely...
            var writeableBitmap = new WriteableBitmap(bitmapDecoder.Frames.Single());
            writeableBitmap.Freeze();

            return writeableBitmap;
        }
    }
}
