using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels;

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

        public MainView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the OnSizeChanged event of the MainWindow control (when the user resized the <see cref="Window"/>).
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ConvosListControl.Height = this.ActualHeight - ProfileStackPanel.ActualHeight - 35.0d;
        }

        /// <summary>
        /// Handles the OnClick event of the sidebar collapse button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CollapseButton_OnClick(object sender, RoutedEventArgs e)
        {
            double width = LeftColumn.ActualWidth;
            if (width > 0) sidebarWidth = width;

            LeftColumn.MinWidth = width > 0 ? 0 : MainViewModel.SIDEBAR_MIN_WIDTH;
            LeftColumn.Width = new GridLength(width > 0 ? 0 : sidebarWidth);

            UpdateCollapseButtonContent(sender as Button);
        }

        /// <summary>
        /// Handles the OnDragStarted event of the GridSplitter control (the one that drives the sidebar's width).
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragStartedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the OnMouseDown event of the SubscriptionProgressBar control
        /// (when the user clicks on his remaining time, he should be redirected to the website where he can extend his subscription).
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void SubscriptionProgressBar_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.glitchedpolygons.com/extend-epistle-sub");
        }

        /// <summary>
        /// Handles the OnMouseEnter event of the SubscriptionProgressBar control
        /// (highlight the bar on mouse enter to notify the user that the progress bar is clickable).
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void SubscriptionProgressBar_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is ProgressBar progressBar)
            {
                progressBar.Foreground = PROGRESS_BAR_COLOR_HOVER;
            }
        }

        /// <summary>
        /// Handles the OnMouseLeave event of the SubscriptionProgressBar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void SubscriptionProgressBar_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is ProgressBar progressBar)
            {
                progressBar.Foreground = PROGRESS_BAR_COLOR;
            }
        }

        private void ResetWindowButton_Click(object sender, RoutedEventArgs e) => CollapseButton.Content = "<";
    }
}
