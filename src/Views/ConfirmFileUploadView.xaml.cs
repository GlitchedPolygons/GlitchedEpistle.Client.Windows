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
