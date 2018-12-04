using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const double LEFT_COLUMN_MIN_WIDTH = 300;
        double leftColumnWidth = LEFT_COLUMN_MIN_WIDTH;

        public MainWindow()
        {
            InitializeComponent();
            LeftColumn.MinWidth = LEFT_COLUMN_MIN_WIDTH;
        }

        private void ButtonCollapseConvosList_OnClick(object sender, RoutedEventArgs e)
        {
            double width = LeftColumn.ActualWidth;
            if (width > 0)
            {
                leftColumnWidth = width;
                LeftColumn.MinWidth = 0;
                LeftColumn.Width = new GridLength(0);
            }
            else
            {
                LeftColumn.MinWidth = LEFT_COLUMN_MIN_WIDTH;
                LeftColumn.Width = new GridLength(leftColumnWidth);
            }
        }

        void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ConvosScrollViewer.Height = this.ActualHeight - ProfileStackPanel.ActualHeight - 35;
        }

        void GridSplitter_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            LeftColumn.MinWidth = LEFT_COLUMN_MIN_WIDTH;
        }
    }
}
