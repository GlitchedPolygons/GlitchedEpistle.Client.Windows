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
using System.Windows.Controls;

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.UserControls
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            UserIdTextBox.GotFocus += (_, __) => UserIdTextBox.SelectAll();
            PasswordBox.GotFocus += (_, __) => PasswordBox.SelectAll();
            TotpTextBox.GotFocus += (_, __) => TotpTextBox.SelectAll();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (UserIdTextBox.Text.NullOrEmpty())
            {
                UserIdTextBox.Focus();
            }
            else
            {
                PasswordBox.Focus();
            }
            RegisterButton.IsEnabled = UserIdTextBox.Text.NullOrEmpty();
        }

        private void UserIdTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            LoginButton.IsEnabled = FormReady;
            RegisterButton.IsEnabled = UserIdTextBox.Text.NullOrEmpty();
        }

        private void PasswordTextBox_OnTextChanged(object sender, RoutedEventArgs e)
        {
            LoginButton.IsEnabled = FormReady;
        }

        private void TotpTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            LoginButton.IsEnabled = FormReady;
        }

        private bool FormReady => UserIdTextBox.Text.NotNullNotEmpty()
                                  && PasswordBox.Password.NotNullNotEmpty()
                                  && TotpTextBox.Text.NotNullNotEmpty();
    }
}
