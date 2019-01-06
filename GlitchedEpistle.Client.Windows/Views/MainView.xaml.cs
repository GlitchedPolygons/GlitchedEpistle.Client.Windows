using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Factories;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels;
using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private double sidebarWidth = MainViewModel.SIDEBAR_MIN_WIDTH;

        private static readonly SolidColorBrush PROGRESS_BAR_COLOR = new SolidColorBrush(new Color { R = 8, G = 175, B = 226, A = 255 });
        private static readonly SolidColorBrush PROGRESS_BAR_COLOR_HOVER = new SolidColorBrush(new Color { R = 100, G = 200, B = 226, A = 255 });

        private SettingsView settingsView;

        public MainView(ISettings settings, IEventAggregator eventAggregator, IWindowFactory windowFactory)
        {
            DataContext = new MainViewModel(settings, eventAggregator, windowFactory);
            InitializeComponent();
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ConvosScrollViewer.Height = this.ActualHeight - ProfileStackPanel.ActualHeight - 35;
        }

        private void ButtonCollapseConvosList_OnClick(object sender, RoutedEventArgs e)
        {
            double width = LeftColumn.ActualWidth;
            if (width > 0) sidebarWidth = width;

            LeftColumn.MinWidth = width > 0 ? 0 : MainViewModel.SIDEBAR_MIN_WIDTH;
            LeftColumn.Width = new GridLength(width > 0 ? 0 : sidebarWidth);

            UpdateCollapseButtonContent(sender as Button);
        }

        private void GridSplitter_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            LeftColumn.MinWidth = MainViewModel.SIDEBAR_MIN_WIDTH;
            UpdateCollapseButtonContent(CollapseButton);
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
    }
}
