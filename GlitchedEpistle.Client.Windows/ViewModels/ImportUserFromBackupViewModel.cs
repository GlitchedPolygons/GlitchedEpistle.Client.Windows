using System;
using System.IO;
using System.IO.Compression;
using System.Timers;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Symmetric;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class ImportUserFromBackupViewModel : ViewModel, ICloseable
    {
        #region Constants
        private const double ERROR_MESSAGE_INTERVAL = 7000;
        private readonly Timer errorMessageTimer = new Timer(ERROR_MESSAGE_INTERVAL) { AutoReset = true };

        // Injections:
        private readonly ILogger logger;
        private readonly ISymmetricCryptography aes;
        #endregion

        #region Events        
        /// <summary>
        /// Occurs when the <see cref="ImportUserFromBackupViewModel"/> is requested to be closed
        /// (raise this <see langword="event"/> in this <see langword="class"/> here to request the related <see cref="Window"/>'s closure).
        /// </summary>
        public event EventHandler<EventArgs> RequestedClose;
        #endregion

        #region Commands
        public ICommand CancelCommand { get; }
        public ICommand ImportCommand { get; }
        #endregion

        #region UI Bindings
        private string backupFilePath;
        public string BackupFilePath { get => backupFilePath; set => Set(ref backupFilePath, value); }

        private string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; set => Set(ref errorMessage, value); }
        #endregion

        public ImportUserFromBackupViewModel(ISymmetricCryptography aes, ILogger logger)
        {
            this.aes = aes;
            this.logger = logger;

            CancelCommand = new DelegateCommand(_ => RequestedClose?.Invoke(this, EventArgs.Empty));

            ImportCommand = new DelegateCommand(async commandParam =>
            {
                string pw = null;

                if (commandParam is PasswordBox passwordBox)
                {
                    pw = passwordBox.Password;
                }

                if (File.Exists(BackupFilePath))
                {
                    string path = BackupFilePath;
                    try
                    {
                        if (!string.IsNullOrEmpty(pw))
                        {
                            path = Path.GetTempFileName();
                            byte[] decr = await Task.Run(() => aes.DecryptWithPassword(File.ReadAllBytes(BackupFilePath), pw));
                            if (decr == null || decr.Length == 0)
                            {
                                errorMessageTimer.Stop(); errorMessageTimer.Start();
                                ErrorMessage = "Import procedure failed: couldn't decrypt backup file. Wrong password?";
                                return;
                            }
                            File.WriteAllBytes(path, decr);
                        }

                        // Epistle folder needs to be empty before a backup can be imported.
                        var dir = new DirectoryInfo(Paths.ROOT_DIRECTORY);
                        if (dir.Exists)
                        {
                            dir.DeleteRecursively();
                        }
                        dir.Create();

                        // Unzip the backup into the epistle user directory.
                        ZipFile.ExtractToDirectory(path, Paths.ROOT_DIRECTORY);

                        // Epistle needs to restart in order for the changes to be applied.
                        Application.Current.Shutdown();
                        Process.Start(Application.ResourceAssembly.Location);
                    }
                    catch (Exception e)
                    {
                        errorMessageTimer.Stop(); errorMessageTimer.Start();
                        logger.LogError($"{nameof(ImportUserFromBackupViewModel)}::{nameof(ImportCommand)}: User account import from backup procedure failed; thrown exception: {e.Message}");
                        ErrorMessage = "Import procedure failed: perhaps double check that password (if the backup has one) and make sure that you selected the right file...";
                    }
                    finally
                    {
                        if (!string.IsNullOrEmpty(pw) && path != BackupFilePath && File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                }
                else
                {
                    errorMessageTimer.Stop(); errorMessageTimer.Start();
                    ErrorMessage = "No backup file path specified; nothing to import!";
                }
            });

            errorMessageTimer.Elapsed += (_, __) => ErrorMessage = null;
            errorMessageTimer.Start();
        }
    }
}
