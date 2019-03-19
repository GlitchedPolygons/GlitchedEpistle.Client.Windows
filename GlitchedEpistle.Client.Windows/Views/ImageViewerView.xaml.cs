using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for ImageViewer.xaml
    /// </summary>
    public partial class ImageViewerView : Window
    {
        private const double SCALE_FACTOR = 1.1d;

        private Point start;
        private Point origin;

        public ImageViewerView()
        {
            InitializeComponent();

            TransformGroup transformGroup = new TransformGroup();
            ScaleTransform scaleTransform = new ScaleTransform();
            TranslateTransform translateTransform = new TranslateTransform();

            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(translateTransform);

            Image.RenderTransform = transformGroup;

            Image.MouseWheel += Image_OnMouseWheel;
            Image.MouseLeftButtonDown += Image_OnMouseLeftButtonDown;
            Image.MouseLeftButtonUp += Image_OnMouseLeftButtonUp;
            Image.MouseMove += Image_OnMouseMove;
        }

        private void Image_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point position = e.MouseDevice.GetPosition(Image);
            Matrix matrix = Image.RenderTransform.Value;

            double scaleXY = e.Delta > 0.0d ? SCALE_FACTOR : 1.0d / SCALE_FACTOR;

            matrix.ScaleAtPrepend(scaleXY, scaleXY, position.X, position.Y);

            Image.RenderTransform = new MatrixTransform(matrix);
        }

        private void Image_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!Image.IsMouseCaptured)
            {
                return;
            }

            Point position = e.MouseDevice.GetPosition(Border);

            Matrix matrix = Image.RenderTransform.Value;
            matrix.OffsetX = origin.X + (position.X - start.X);
            matrix.OffsetY = origin.Y + (position.Y - start.Y);

            Image.RenderTransform = new MatrixTransform(matrix);
        }

        private void Image_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Image.IsMouseCaptured)
            {
                return;
            }

            Image.CaptureMouse();

            start = e.GetPosition(Border);
            origin.X = Image.RenderTransform.Value.OffsetX;
            origin.Y = Image.RenderTransform.Value.OffsetY;
        }

        private void Image_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image.ReleaseMouseCapture();
        }
    }
}
