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

using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
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

        public MainView(IEventAggregator eventAggregator)
        {
            eventAggregator?.GetEvent<JoinedConvoEvent>()?.Subscribe(OnJoinedConvo);
            InitializeComponent();
        }

        private void OnJoinedConvo(Convo convo)
        {
            CollapseButton_OnClick(null, null);
        }

        /// <summary>
        /// Handles the OnSizeChanged event of the MainWindow control (when the user resized the <see cref="Window"/>).
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ConvosListControl.Height = ActualHeight - ProfileStackPanel.ActualHeight - 35.0d;
        }

        /// <summary>
        /// Handles the OnClick event of the sidebar collapse button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CollapseButton_OnClick(object sender, RoutedEventArgs e)
        {
            double width = LeftColumn.ActualWidth;
            bool expanded = width > 0;
            if (expanded)
            {
                sidebarWidth = width;
            }

            LeftColumn.MinWidth = expanded ? 0 : MainViewModel.SIDEBAR_MIN_WIDTH;
            LeftColumn.Width = new GridLength(expanded ? 0 : sidebarWidth);

            UpdateCollapseButtonContent(CollapseButton);
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
            {
                return;
            }

            button.Content = LeftColumn.MinWidth > 0 ? "<" : ">";
        }

        /// <summary>
        /// Handles the OnMouseDown event of the ProgressBar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void SubscriptionProgressBar_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/GlitchedPolygons/GlitchedEpistle.Client.Windows");
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

        private void ResetWindowButton_Click(object sender, RoutedEventArgs e)
        {
            CollapseButton.Content = "<";
        }
    }
}
