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
using System.Windows.Controls;
using System.Windows.Input;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.UserControls
{
    /// <summary>
    /// Interaction logic for RegistrationSuccessfulView.xaml
    /// </summary>
    public partial class UserCreationSuccessfulView : UserControl
    {
        public UserCreationSuccessfulView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            // Immediately focus the 2FA secret textbox when the control is loaded.
            SecretTextBox?.Focus();
            SecretTextBox?.SelectAll();
        }

        private void TotpTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            VerifyButton.IsEnabled = TotpTextBox.Text.Length == 6;
        }

        private void SecretTextBox_SelectText(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox?.SelectAll();
        }

        private void SecretTextBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var textBox = sender as TextBox;

            if (textBox == null || textBox.IsKeyboardFocusWithin)
            {
                return;
            }

            e.Handled = true;
            textBox.Focus();
        }
    }
}
