using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Symmetric;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class ImportUserFromBackupViewModel : ViewModel, ICloseable
    {
        #region Constants
        private const double ERROR_MESSAGE_INTERVAL = 7000;
        private readonly Timer messageResetTimer = new Timer(ERROR_MESSAGE_INTERVAL) { AutoReset = true };

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

        private string successMessage = string.Empty;
        public string SuccessMessage { get => successMessage; set => Set(ref successMessage, value); }

        private bool uiEnabled = true;
        public bool UIEnabled { get => uiEnabled; set => Set(ref uiEnabled, value); }
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
                    UIEnabled = false;
                    messageResetTimer.Stop();
                    messageResetTimer.Start();
                    ErrorMessage = null;
                    SuccessMessage = "Importing user account from backup...";

                    string path = BackupFilePath;
                    try
                    {
                        if (pw.NotNullNotEmpty())
                        {
                            path = Path.GetTempFileName();
                            byte[] decr = await Task.Run(() => aes.DecryptWithPassword(File.ReadAllBytes(BackupFilePath), pw));
                            if (decr == null || decr.Length == 0)
                            {
                                messageResetTimer.Stop();
                                messageResetTimer.Start();
                                SuccessMessage = null;
                                ErrorMessage = "Import procedure failed: couldn't decrypt backup file. Wrong password?";
                                UIEnabled = true;
                                return;
                            }
                            File.WriteAllBytes(path, decr);
                        }

                        await Task.Run(() =>
                        {
                            // Epistle folder needs to be empty before a backup can be imported.
                            DirectoryInfo dir = new DirectoryInfo(Paths.ROOT_DIRECTORY);
                            if (dir.Exists)
                            {
                                dir.DeleteRecursively();
                            }
                            dir.Create();

                            // Unzip the backup into the epistle user directory.
                            ZipFile.ExtractToDirectory(path, Paths.ROOT_DIRECTORY);
                        });

                        // Epistle needs to restart in order for the changes to be applied.
                        Application.Current.Shutdown();
                        Process.Start(Application.ResourceAssembly.Location);
                    }
                    catch (Exception e)
                    {
                        messageResetTimer.Stop();
                        messageResetTimer.Start();
                        logger.LogError($"{nameof(ImportUserFromBackupViewModel)}::{nameof(ImportCommand)}: User account import from backup procedure failed; thrown exception: {e.Message}");
                        SuccessMessage = null;
                        UIEnabled = true;
                        ErrorMessage = "Import procedure failed: perhaps double check that password (if the backup has one) and make sure that you selected the right file...";
                    }
                    finally
                    {
                        if (pw.NotNullNotEmpty() && path != BackupFilePath && File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                }
                else
                {
                    messageResetTimer.Stop();
                    messageResetTimer.Start();
                    ErrorMessage = "No backup file path specified; nothing to import!";
                    SuccessMessage = null;
                }
            });

            messageResetTimer.Elapsed += (_, __) => ErrorMessage = SuccessMessage = null;
            messageResetTimer.Start();
        }
    }
}
