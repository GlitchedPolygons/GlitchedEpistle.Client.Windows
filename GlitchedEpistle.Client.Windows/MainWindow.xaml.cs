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
        Settings settings;

        public MainWindow()
        {
            InitializeComponent();
            LeftColumn.MinWidth = LEFT_COLUMN_MIN_WIDTH;
            UpdateCollapseButtonContent(CollapseButton);
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            settings?.Close();
        }

        void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ConvosScrollViewer.Height = this.ActualHeight - ProfileStackPanel.ActualHeight - 35;
        }

        void ButtonCollapseConvosList_OnClick(object sender, RoutedEventArgs e)
        {
            double width = LeftColumn.ActualWidth;
            if (width > 0)
            {
                leftColumnWidth = width;
            }

            LeftColumn.MinWidth = width > 0 ? 0 : LEFT_COLUMN_MIN_WIDTH;
            LeftColumn.Width = new GridLength(width > 0 ? 0 : leftColumnWidth);

            UpdateCollapseButtonContent(e.Source as Button);
        }

        void GridSplitter_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            LeftColumn.MinWidth = LEFT_COLUMN_MIN_WIDTH;
            UpdateCollapseButtonContent(CollapseButton);
        }

        void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (settings == null)
            {
                settings = new Settings();
                settings.Closed += SettingsWindow_OnClosed;
            }

            settings.Show();
        }

        void SettingsWindow_OnClosed(object sender, EventArgs e)
        {
            settings = null;
        }

        /// <summary>
        /// Checks whether the left column is collapsed or not
        /// and applies the &lt; or &gt; symbol to the button's content accordingly.
        /// </summary>
        /// <param name="button">The collapse button to update.</param>
        void UpdateCollapseButtonContent(Button button)
        {
            if (button is null)
                return;

            button.Content = LeftColumn.MinWidth > 0 ? "<" : ">";
        }
    }
}
