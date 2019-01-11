using System;
using System.Windows;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Extensions;
using Microsoft.Win32;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for UserExportDialog.xaml
    /// </summary>
    public partial class UserExportView : Window
    {
        public UserExportView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.MakeCloseable();
            Loaded -= OnLoaded;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Epistle backup file path",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                FileName = $"{DateTime.Now:yyyy-MM-dd-HH-mm}-glitched-epistle-backup.dat",
                DefaultExt = ".dat",
                AddExtension = true,
                OverwritePrompt = true,
                Filter = "Epistle Backup File|*.dat"
            };
            dialog.FileOk += FileDialog_FileOk;
            dialog.ShowDialog(this);
        }

        private void FileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is SaveFileDialog dialog)
            {
                dialog.FileOk -= FileDialog_FileOk;
                OutputFilePathTextBox.Text = dialog.FileName;
            }
        }
    }
}
