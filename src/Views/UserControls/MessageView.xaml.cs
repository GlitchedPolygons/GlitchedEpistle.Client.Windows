/*
    Glitched Epistle - Windows Client
    Copyright (C) 2019 Raphael Beck

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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.UserControls
{
    /// <summary>
    /// Interaction logic for MessageView.xaml
    /// </summary>
    public partial class MessageView : UserControl
    {
        public MessageView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            TextBox.TextChanged += TextBox_TextChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            TextBox_TextChanged(null, null);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox.Visibility = string.IsNullOrEmpty(TextBox.Text) ? Visibility.Hidden : Visibility.Visible;
        }

        // Select the text inside the textbox on click.
        private void TextBox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null || textBox.IsKeyboardFocusWithin)
            {
                return;
            }

            e.Handled = true;
            textBox.Focus();
        }

        private void TextBox_SelectText(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox?.SelectAll();
        }

        private void AudioMessageSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            (DataContext as MessageViewModel)?.OnAudioThumbDragged();
        }
    }
}
