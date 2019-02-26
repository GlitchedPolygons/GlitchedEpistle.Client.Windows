using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows;
using System.Windows.Input;

using GlitchedPolygons.Services.CompressionUtility;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;

using Microsoft.Win32;
using Newtonsoft.Json;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class UserExportViewModel : ViewModel, ICloseable
    {
        #region Constants
        private const bool DEFAULT_EXPORT_SETTINGS = true;
        private const bool DEFAULT_EXPORT_CONVOS = false;
        private const bool DEFAULT_COMPRESS_OUTPUT = false;

        // Injections:
        private readonly User user;
        private readonly ISettings settings;
        private readonly ICompressionUtility gzip;
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
        private string outputFilePath;
        public string OutputFilePath { get => outputFilePath; set => Set(ref outputFilePath, value); }

        private string exportLabel;
        public string ExportLabel { get => exportLabel; set => Set(ref exportLabel, value); }

        private bool exportSettings = DEFAULT_EXPORT_SETTINGS;
        public bool ExportSettings
        {
            get => exportSettings;
            set
            {
                Set(ref exportSettings, value);
                UpdateExportLabel();
            }
        }

        private bool exportConvos = DEFAULT_EXPORT_CONVOS;
        public bool ExportConvos
        {
            get => exportConvos;
            set
            {
                Set(ref exportConvos, value);
                UpdateExportLabel();
            }
        }

        private bool compressOutput = DEFAULT_COMPRESS_OUTPUT;
        public bool CompressOutput
        {
            get => compressOutput;
            set
            {
                Set(ref compressOutput, value);
                UpdateExportLabel();
            }
        }

        private bool exportReady;
        public bool ExportReady { get => exportReady; set => Set(ref exportReady, value); }
        #endregion

        public UserExportViewModel(ISettings settings, User user, ICompressionUtility gzip)
        {
            this.user = user;
            this.gzip = gzip;
            this.settings = settings;

            ClosedCommand = new DelegateCommand(OnClosed);
            BrowseButtonCommand = new DelegateCommand(OnClickedBrowse);
            CancelButtonCommand = new DelegateCommand(OnClickedCancel);
            ExportButtonCommand = new DelegateCommand(OnClickedExport);

            settings.Load();
            UpdateExportLabel();
        }

        private void UpdateExportLabel()
        {
            string verb = compressOutput ? "gzipped and exported" : "exported";
            if (!exportSettings && !exportConvos)
                ExportLabel = $"Only your raw user id and key pair will be {verb}; no convos, no settings!";
            else if (exportSettings && !exportConvos)
                ExportLabel = $"Your user credentials and settings will be {verb} without the convos.";
            else if (exportSettings && exportConvos)
                ExportLabel = $"Everything including your convos will be {verb}. Please note that this might take a while and end up using a lot of space!";
            else if (!exportSettings && exportConvos)
                ExportLabel = $"Your user credentials and convos will be {verb} without the user settings.";
        }

        private void OnClosed(object commandParam)
        {
            // nop
        }

        private void OnClickedBrowse(object commandParam)
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
            dialog.ShowDialog();
        }

        private void FileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is SaveFileDialog dialog)
            {
                dialog.FileOk -= FileDialog_FileOk;
                OutputFilePath = dialog.FileName;
            }
            ExportReady = !string.IsNullOrEmpty(OutputFilePath);
        }

        private void OnClickedCancel(object commandParam)
        {
            RequestedClose?.Invoke(this, EventArgs.Empty);
        }

        private void OnClickedExport(object commandParam)
        {
            object userSettings = null;
            if (ExportSettings)
            {
                userSettings = new
                {
                    // TODO: gather settings here
                };
            }

            object userConvos = null;
            if (ExportConvos)
            {
                userConvos = new
                {
                    // TODO: gather convos here (maybe use Parallel.For or something multi-threaded...)
                };
            }

            object output = new
            {
                User = new
                {
                    UserId = user.Id,
                    Username = settings["Username"],
                    PublicKeyXml = user.PublicKeyXml,
                    PrivateKeyXml = user.PrivateKey.ToXmlString()
                },
                UserSettings = userSettings,
                UserConvos = userConvos
            };

            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(output));
            byte[] gzippedBytes = CompressOutput ? gzip.Compress(bytes, new CompressionSettings { CompressionLevel = CompressionLevel.Fastest }) : Array.Empty<byte>();

            File.WriteAllBytes(
               path: CompressOutput ? OutputFilePath + ".gz" : OutputFilePath, 
               bytes: CompressOutput ? gzippedBytes : bytes
            );

            for (int i = 0; i < bytes.Length; i++) bytes[i] = 0;
            for (int i = 0; i < gzippedBytes.Length; i++) gzippedBytes[i] = 0;
        }
    }
}
