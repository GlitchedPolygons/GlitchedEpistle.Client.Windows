using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double LEFT_COLUMN_MIN_WIDTH = 300;
        private double leftColumnWidth = LEFT_COLUMN_MIN_WIDTH;

        private readonly App app;
        private readonly ISettings settings;

        private static readonly SolidColorBrush PROGRESS_BAR_COLOR = new SolidColorBrush(new Color { R = 8, G = 175, B = 226, A = 255 });
        private static readonly SolidColorBrush PROGRESS_BAR_COLOR_HOVER = new SolidColorBrush(new Color { R = 100, G = 200, B = 226, A = 255 });

        private SettingsWindow settingsWindow;

        public MainWindow(ISettings settings)
        {
            this.settings = settings;

            InitializeComponent();
            LeftColumn.MinWidth = LEFT_COLUMN_MIN_WIDTH;
            UpdateCollapseButtonContent(CollapseButton);
            app = Application.Current as App;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            settingsWindow?.Close();
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ConvosScrollViewer.Height = this.ActualHeight - ProfileStackPanel.ActualHeight - 35;
        }

        private void ButtonCollapseConvosList_OnClick(object sender, RoutedEventArgs e)
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

        private void GridSplitter_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            LeftColumn.MinWidth = LEFT_COLUMN_MIN_WIDTH;
            UpdateCollapseButtonContent(CollapseButton);
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            settingsWindow = app.GetWindow<SettingsWindow>(true);
            settingsWindow.Show();
            settingsWindow.Activate();
        }

        /// <summary>
        /// Checks whether the left column is collapsed or not
        /// and applies the &lt; or &gt; symbol to the button's content accordingly.
        /// </summary>
        /// <param name="button">The collapse button to update.</param>
        private void UpdateCollapseButtonContent(Button button)
        {
            if (button is null)
                return;

            button.Content = LeftColumn.MinWidth > 0 ? "<" : ">";
        }

        private void SubscriptionProgressBar_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.glitchedpolygons.com/extend-epistle-sub");
        }

        private void SubscriptionProgressBar_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is ProgressBar progressBar)
            {
                progressBar.Foreground = PROGRESS_BAR_COLOR_HOVER;
                // TODO: update the tooltip here with the user
            }
        }

        private void SubscriptionProgressBar_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is ProgressBar progressBar)
            {
                progressBar.Foreground = PROGRESS_BAR_COLOR;
            }
        }

        private void CreateConvoButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ChangePasswordButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
