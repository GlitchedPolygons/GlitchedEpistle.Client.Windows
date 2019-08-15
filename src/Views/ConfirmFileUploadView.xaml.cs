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

using System;
using System.IO;
using System.Windows;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for ConfirmFileUploadView.xaml
    /// </summary>
    public partial class ConfirmFileUploadView : Window
    {
        private readonly string filePath;

        public ConfirmFileUploadView(string filePath)
        {
            InitializeComponent();
            this.filePath = filePath;
            FilePathLabel.Text = filePath;
            FileNameLabel.Text = Path.GetFileName(filePath);
            Focus();
        }

        private void ExploreButton_Click(object sender, EventArgs e)
        {
            if (!File.Exists(filePath))
            {
                return;
            }
            
            System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + filePath + "\"");
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
