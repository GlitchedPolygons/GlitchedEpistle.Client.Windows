﻿using System;
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
        public string BackupFilePath
        {
            get => backupFilePath;
            set => Set(ref backupFilePath, value);
        }

        private string errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => errorMessage;
            set => Set(ref errorMessage, value);
        }

        private string successMessage = string.Empty;
        public string SuccessMessage
        {
            get => successMessage;
            set => Set(ref successMessage, value);
        }

        private bool uiEnabled = true;
        public bool UIEnabled
        {
            get => uiEnabled;
            set => Set(ref uiEnabled, value);
        }
        #endregion

        private bool importing;

        public ImportUserFromBackupViewModel(ISymmetricCryptography aes, ILogger logger)
        {
            this.aes = aes;
            this.logger = logger;

            ImportCommand = new DelegateCommand(OnClickedImport);
            CancelCommand = new DelegateCommand(_ => RequestedClose?.Invoke(this, EventArgs.Empty));

            messageResetTimer.Elapsed += (_, __) => ErrorMessage = SuccessMessage = null;
            messageResetTimer.Start();
        }

        private void OnClickedImport(object commandParam)
        {
            if (importing)
            {
                return;
            }

            string pw = null;

            if (commandParam is PasswordBox passwordBox)
            {
                pw = passwordBox.Password;
            }

            if (!File.Exists(BackupFilePath))
            {
                ResetMessages();
                SuccessMessage = null;
                ErrorMessage = "No backup file path specified; nothing to import!";
                return;
            }

            importing = true;
            UIEnabled = false;
            ResetMessages();
            SuccessMessage = "Importing user account from backup...";

            Task.Run(async () =>
            {
                string path = BackupFilePath;
                try
                {
                    // If the user entered a password,
                    // attempt to decrypt the backup file with it.
                    if (pw.NotNullNotEmpty())
                    {
                        path = await Task.Run(() =>
                        {
                            // The decrypted backup will be stored in a temporary path.
                            string tempPath = Path.GetTempFileName();
                            byte[] decr = aes.DecryptWithPassword(File.ReadAllBytes(BackupFilePath), pw);
                            if (decr == null || decr.Length == 0)
                            {
                                return null;
                            }
                            File.WriteAllBytes(tempPath, decr);
                            return tempPath;
                        });

                        if (path.NullOrEmpty())
                        {
                            importing = false;
                            Application.Current?.Dispatcher?.Invoke(() =>
                            {
                                UIEnabled = true;
                                ResetMessages();
                                ErrorMessage = "Import procedure failed: couldn't decrypt backup file. Wrong password?";
                            });
                            return;
                        }
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
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        Application.Current.Shutdown();
                        Process.Start(Application.ResourceAssembly.Location);
                    });
                }
                catch (Exception e)
                {
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        ResetMessages();
                        ErrorMessage = "Import procedure failed: perhaps double check that password (if the backup has one) and make sure that you selected the right file...";
                        logger.LogError($"{nameof(ImportUserFromBackupViewModel)}::{nameof(ImportCommand)}: User account import from backup procedure failed; thrown exception: {e.Message}");
                    });
                }
                finally
                {
                    if (pw.NotNullNotEmpty() && path != BackupFilePath && File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    importing = false;
                    Application.Current?.Dispatcher?.Invoke(() => UIEnabled = true);
                }
            });
        }

        private void ResetMessages()
        {
            messageResetTimer.Stop();
            messageResetTimer.Start();
            ErrorMessage = SuccessMessage = null;
        }
    }
}
