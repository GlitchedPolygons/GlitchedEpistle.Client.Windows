using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Extensions
{
    public static class BitmapExtensions
    {
        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            if (Application.Current.Dispatcher == null)
                return null; // Is this even possible?

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    // You need to specify the image format to fill the stream. Let's hope this is always PNG ;D
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // Make sure to create the bitmap in the UI thread...
                    if (InvokeRequired)
                    {
                        return (BitmapSource)Application.Current.Dispatcher.Invoke(
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

        private static bool InvokeRequired => Dispatcher.CurrentDispatcher != Application.Current.Dispatcher;

        private static BitmapSource CreateBitmapSourceFromBitmap(Stream stream)
        {
            BitmapDecoder bitmapDecoder = BitmapDecoder.Create(
                bitmapStream: stream,
                cacheOption: BitmapCacheOption.OnLoad,
                createOptions: BitmapCreateOptions.PreservePixelFormat
            );

            // This will disconnect the stream from the image completely...
            WriteableBitmap writable = new WriteableBitmap(bitmapDecoder.Frames.Single());
            writable.Freeze();

            return writable;
        }
    }
}
