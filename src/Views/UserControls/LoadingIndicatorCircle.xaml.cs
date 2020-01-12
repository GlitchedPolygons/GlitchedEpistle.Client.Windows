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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.UserControls
{
    /// <summary>
    /// Interaction logic for LoadingIndicatorCircle.xaml
    /// </summary>
    public partial class LoadingIndicatorCircle : UserControl
    {
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(int), typeof(LoadingIndicatorCircle), new UIPropertyMetadata(1));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(int), typeof(LoadingIndicatorCircle), new UIPropertyMetadata(1));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(LoadingIndicatorCircle), new UIPropertyMetadata(100));

        /// <summary>
        /// Gets or sets the minimum.
        /// </summary>
        /// <value>The minimum.</value>
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        /// <value>The maximum.</value>
        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private readonly DispatcherTimer animationTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingIndicatorCircle"/> class.
        /// </summary>
        public LoadingIndicatorCircle()
        {
            InitializeComponent();

            IsVisibleChanged += OnVisibleChanged;

            animationTimer = new DispatcherTimer(DispatcherPriority.Render, Dispatcher)
            {
                Interval = new TimeSpan(0, 0, 0, 0, 75)
            };
        }

        /// <summary>
        /// Sets the position.
        /// </summary>
        /// <param name="ellipse">The ellipse.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="posOffset">The positional offset.</param>
        /// <param name="step">The step to change.</param>
        private static void SetPosition(DependencyObject ellipse, double offset, double posOffset, double step)
        {
            ellipse.SetValue(Canvas.LeftProperty, 50 + (Math.Sin(offset + (posOffset * step)) * 50));
            ellipse.SetValue(Canvas.TopProperty, 50 + (Math.Cos(offset + (posOffset * step)) * 50));
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        private void Start()
        {
            animationTimer.Tick += OnAnimationTick;
            animationTimer.Start();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        private void Stop()
        {
            animationTimer.Stop();
            animationTimer.Tick -= OnAnimationTick;
        }

        /// <summary>
        /// Handles the animation tick.
        /// </summary>
        /// <param name="sender">The sender who raised the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnAnimationTick(object sender, EventArgs e)
        {
            SpinnerRotate.Angle = (SpinnerRotate.Angle + 36) % 360;
        }

        /// <summary>
        /// Handles the loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnCanvasLoaded(object sender, RoutedEventArgs e)
        {
            const double OFFSET = Math.PI;
            const double STEP = Math.PI * 2.0 / 10.0;

            SetPosition(Circle0, OFFSET, 0.0, STEP);
            SetPosition(Circle1, OFFSET, 1.0, STEP);
            SetPosition(Circle2, OFFSET, 2.0, STEP);
            SetPosition(Circle3, OFFSET, 3.0, STEP);
            SetPosition(Circle4, OFFSET, 4.0, STEP);
            SetPosition(Circle5, OFFSET, 5.0, STEP);
            SetPosition(Circle6, OFFSET, 6.0, STEP);
            SetPosition(Circle7, OFFSET, 7.0, STEP);
            SetPosition(Circle8, OFFSET, 8.0, STEP);
        }

        /// <summary>
        /// Handles the canvas unloaded event.
        /// </summary>
        /// <param name="sender">The sending entity.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnCanvasUnloaded(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        /// <summary>
        /// Handles the visibility changed event.
        /// </summary>
        /// <param name="sender">The sender who raised this event.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }
    }
}
