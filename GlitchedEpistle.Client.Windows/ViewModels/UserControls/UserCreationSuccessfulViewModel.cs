﻿using System;
using System.IO;
using System.Text;
using System.Timers;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

using Prism.Events;
using Microsoft.Win32;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class UserCreationSuccessfulViewModel : ViewModel
    {
        #region Constants
        private readonly User user;
        private readonly ISettings settings;
        private readonly IUserService userService;
        private readonly IEventAggregator eventAggregator;
        private const double ERROR_MESSAGE_INTERVAL = 7000;
        #endregion

        #region Commands
        public ICommand ExportBackupCodesCommand { get; }
        public ICommand VerifyCommand { get; }
        #endregion

        #region UI Bindings
        private string totp = string.Empty;
        public string Totp { get => totp; set => Set(ref totp, value); }

        private string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; set => Set(ref errorMessage, value); }

        private BitmapSource qr;
        public BitmapSource QR { get => qr; set => Set(ref qr, value); }
        #endregion

        private bool pendingAttempt = false;
        private Timer ErrorMessageTimer { get; } = new Timer(ERROR_MESSAGE_INTERVAL) { AutoReset = true };

        public List<string> BackupCodes { get; set; }

        public UserCreationSuccessfulViewModel(ISettings settings, IUserService userService, IEventAggregator eventAggregator, User user)
        {
            this.user = user;
            this.settings = settings;
            this.userService = userService;
            this.eventAggregator = eventAggregator;

            VerifyCommand = new DelegateCommand(OnClickedVerify);
            ExportBackupCodesCommand = new DelegateCommand(OnClickedExport);

            ErrorMessageTimer.Elapsed += ErrorMessageTimer_Elapsed;
            ErrorMessageTimer.Start();
        }

        private void ErrorMessageTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ErrorMessage != null)
                ErrorMessage = null;
        }

        private void OnClickedExport(object commandParam)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Epistle 2FA Backup Codes",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                FileName = "glitched-epistle-2fa-backup-codes.txt",
                DefaultExt = ".txt",
                AddExtension = true,
                OverwritePrompt = true,
                Filter = "Text File|*.txt"
            };
            dialog.FileOk += ExportFileDialog_FileOk;
            dialog.ShowDialog();
        }

        private async void OnClickedVerify(object commandParam)
        {
            if (pendingAttempt)
            {
                return;
            }

            pendingAttempt = true;
            if (await userService.Validate2FA(user.Id, Totp))
            {
                eventAggregator.GetEvent<UserCreationVerifiedEvent>().Publish();
            }
            else
            {
                ErrorMessageTimer.Stop();
                ErrorMessageTimer.Start();
                ErrorMessage = "Two-Factor authentication failed!";
                Totp = string.Empty;
            }
            pendingAttempt = false;
        }

        private void ExportFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is SaveFileDialog dialog)
            {
                dialog.FileOk -= ExportFileDialog_FileOk;
                if (string.IsNullOrEmpty(dialog.FileName) || BackupCodes.Count == 0)
                {
                    return;
                }

                var sb = new StringBuilder(512);
                sb.AppendLine("Glitched Epistle 2FA Backup Codes\n");
                sb.AppendLine($"User: {user.Id}");
                sb.AppendLine($"Export timestamp: {DateTime.UtcNow:s} (UTC)\n");

                for (int i = 0; i < BackupCodes.Count; i++)
                {
                    sb.AppendLine(BackupCodes[i]);
                }

                File.WriteAllText(dialog.FileName, sb.ToString());
            }
        }
    }
}