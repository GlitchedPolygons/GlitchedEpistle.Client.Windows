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

        private void Rotate90degButton_Click(object sender, RoutedEventArgs e)
        {
            Matrix matrix = Image.RenderTransform.Value;
            matrix.Rotate(90);
            Image.RenderTransform = new MatrixTransform(matrix);
        }
    }
}
