using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;

using Microsoft.Win32;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class UserExportViewModel : ViewModel, ICloseable
    {
        #region Constants

        #endregion

        #region Events        
        /// <summary>
        /// Occurs when the <see cref="UserExportView"/> is requested to be closed
        /// (raise this <see langword="event"/> in this <see langword="class"/> here to request the related <see cref="Window"/>'s closure).
        /// </summary>
        public event EventHandler<EventArgs> RequestedClose;
        #endregion

        #region Commands
        public ICommand ClosedCommand { get; }
        public ICommand BrowseButtonCommand { get; }
        public ICommand CancelButtonCommand { get; }
        public ICommand ExportButtonCommand { get; }
        #endregion

        #region UI Bindings
        private bool enabled = true;
        public bool Enabled { get => enabled; set => Set(ref enabled, value); }

        private string outputFilePath;
        public string OutputFilePath { get => outputFilePath; set => Set(ref outputFilePath, value); }

        private string exportLabel = "Export a complete backup of your Epistle account. This includes convos and user settings. You can also choose to encrypt your backup with a password.";
        public string ExportLabel { get => exportLabel; set => Set(ref exportLabel, value); }
        #endregion

        public UserExportViewModel()
        {
            ClosedCommand = new DelegateCommand(OnClosed);

            // On clicked "Browse"
            BrowseButtonCommand = new DelegateCommand(commandParam =>
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

                dialog.FileOk += (sender, e) =>
                {
                    if (sender is SaveFileDialog _dialog)
                    {
                        OutputFilePath = _dialog.FileName;
                    }
                };

                dialog.ShowDialog();
            });

            // On clicked "Export"
            ExportButtonCommand = new DelegateCommand(async commandParam =>
            {
                string pw = null;
                if (commandParam is PasswordBox passwordBox)
                {
                    pw = passwordBox.Password;
                }

                await Task.Run(() =>
                {
                    if (File.Exists(OutputFilePath))
                        File.Delete(OutputFilePath);

                    ZipFile.CreateFromDirectory(Paths.ROOT_DIRECTORY, OutputFilePath, CompressionLevel.Optimal, false);
                });

                if (!File.Exists(OutputFilePath))
                {
                    ExportLabel = $"Backup couldn't be exported to {OutputFilePath}... Reason unknown";
                    OutputFilePath = null;
                    return;
                }

                if (string.IsNullOrEmpty(pw))
                {
                    ExportLabel = "Backup exported successfully! Please keep that file VERY secret (you chose not to encrypt it with a password after all...).";
                    Enabled = false;
                    OutputFilePath = null;
                    return;
                }

                // TODO: encrypt using pw

                Enabled = false;
                OutputFilePath = null;
                ExportLabel = "Backup exported successfully! Please don't share that file with anybody.";
            });

            // On clicked "Cancel"
            CancelButtonCommand = new DelegateCommand(_ => RequestedClose?.Invoke(this, EventArgs.Empty));
        }

        private void OnClosed(object commandParam)
        {
            // nop
        }
    }
}
