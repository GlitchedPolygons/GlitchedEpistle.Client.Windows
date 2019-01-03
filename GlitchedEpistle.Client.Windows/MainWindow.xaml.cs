using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double LEFT_COLUMN_MIN_WIDTH = 340;
        private const double MAIN_WINDOW_MIN_WIDTH = 800;
        private const double MAIN_WINDOW_MIN_HEIGHT = 450;
        private double leftColumnWidth = LEFT_COLUMN_MIN_WIDTH;

        private readonly App app;
        private readonly ISettings settings;

        private static readonly SolidColorBrush PROGRESS_BAR_COLOR = new SolidColorBrush(new Color { R = 8, G = 175, B = 226, A = 255 });
        private static readonly SolidColorBrush PROGRESS_BAR_COLOR_HOVER = new SolidColorBrush(new Color { R = 100, G = 200, B = 226, A = 255 });

        private SettingsWindow settingsWindow;

        public MainWindow(ISettings settings)
        {
            this.settings = settings;
            app = Application.Current as App;

            InitializeComponent();

            // Register events.
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize variables and UI elements.
            MinWidth = MAIN_WINDOW_MIN_WIDTH;
            MinHeight = MAIN_WINDOW_MIN_HEIGHT;
            LeftColumn.MinWidth = LEFT_COLUMN_MIN_WIDTH;

            // Load up the settings on startup.
            if (settings.Load())
            {
                UsernameLabel.Content = settings["Username", "user"];

                Enum.TryParse<WindowState>(settings["WindowState", WindowState.Normal.ToString()], out var windowState);
                WindowState = windowState;

                Width = Math.Abs(settings["MainWindowWidth", MAIN_WINDOW_MIN_WIDTH]);
                Height = Math.Abs(settings["MainWindowHeight", MAIN_WINDOW_MIN_HEIGHT]);

                double sidebarWidth = Math.Abs(settings["SidebarWidth", LEFT_COLUMN_MIN_WIDTH]);
                LeftColumn.Width = new GridLength(sidebarWidth < LEFT_COLUMN_MIN_WIDTH ? LEFT_COLUMN_MIN_WIDTH : sidebarWidth > ActualWidth ? ActualWidth : sidebarWidth);
            }

            UpdateCollapseButtonContent(CollapseButton);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            settingsWindow?.Close();

            var c = CultureInfo.InvariantCulture;
            settings["WindowState"] = WindowState.ToString();
            settings["MainWindowWidth"] = ((int)ActualWidth).ToString(c);
            settings["MainWindowHeight"] = ((int)ActualHeight).ToString(c);
            settings["SidebarWidth"] = ((int)LeftColumn.ActualWidth).ToString(c);
            settings.Save();
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ConvosScrollViewer.Height = this.ActualHeight - ProfileStackPanel.ActualHeight - 35;
        }

        private void ButtonCollapseConvosList_OnClick(object sender, RoutedEventArgs e)
        {
            double width = LeftColumn.ActualWidth;
            if (width > 0) leftColumnWidth = width;

            LeftColumn.MinWidth = width > 0 ? 0 : LEFT_COLUMN_MIN_WIDTH;
            LeftColumn.Width = new GridLength(width > 0 ? 0 : leftColumnWidth);

            UpdateCollapseButtonContent(sender as Button);
        }

        private void GridSplitter_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            LeftColumn.MinWidth = LEFT_COLUMN_MIN_WIDTH;
            UpdateCollapseButtonContent(CollapseButton);
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            settingsWindow = app.GetWindow<SettingsWindow>(true);
            settingsWindow.Closed += SettingsWindow_Closed;
            settingsWindow.Show();
            settingsWindow.Activate();
        }

        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            if (settings.Load())
            {
                UsernameLabel.Content = settings["Username"];
            }
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

        private void LogoutButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ExportUserButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
