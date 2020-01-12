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

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Extensions;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for UsernamePromptView.xaml
    /// </summary>
    public partial class UsernamePromptView : Window
    {
        public UsernamePromptView()
        {
            InitializeComponent();
            UsernameTextBox.Focus();
            Loaded += UsernamePromptView_Loaded;
        }

        private void UsernamePromptView_Loaded(object sender, RoutedEventArgs e)
        {
            UsernameTextBox.Focus();
        }

        private void UsernameTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            this.MakeCloseable();
            AcceptButton.IsEnabled = UsernameTextBox.Text.NotNullNotEmpty();
        }
    }
}
