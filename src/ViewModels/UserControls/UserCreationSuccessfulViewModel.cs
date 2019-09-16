﻿/*
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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;

using Microsoft.Win32;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class UserCreationSuccessfulViewModel : ViewModel
    {
        #region Constants
        private readonly User user;
        private readonly IUserService userService;
        private readonly IEventAggregator eventAggregator;
        private readonly Timer errorMessageTimer = new Timer(ERROR_MESSAGE_INTERVAL) { AutoReset = true };
        private const double ERROR_MESSAGE_INTERVAL = 7000;
        #endregion

        #region Commands
        public ICommand ExportBackupCodesCommand { get; }
        public ICommand VerifyCommand { get; }
        #endregion

        #region UI Bindings
        private string totp = string.Empty;
        public string Totp
        {
            get => totp;
            set => Set(ref totp, value);
        }

        private string errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => errorMessage;
            set => Set(ref errorMessage, value);
        }

        private string secret = string.Empty;
        public string Secret
        {
            get => secret;
            set => Set(ref secret, value);
        }

        private BitmapSource qr;
        public BitmapSource QR
        {
            get => qr;
            set => Set(ref qr, value);
        }
        #endregion

        private bool pendingAttempt;

        public List<string> BackupCodes { get; set; }

        public UserCreationSuccessfulViewModel(IUserService userService, IEventAggregator eventAggregator, User user)
        {
            this.user = user;
            this.userService = userService;
            this.eventAggregator = eventAggregator;

            VerifyCommand = new DelegateCommand(OnClickedVerify);
            ExportBackupCodesCommand = new DelegateCommand(OnClickedExport);

            errorMessageTimer.Elapsed += (_, __) => ErrorMessage = null;
            errorMessageTimer.Start();
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

        private void OnClickedVerify(object commandParam)
        {
            if (pendingAttempt)
            {
                return;
            }

            pendingAttempt = true;

            Task.Run(async () =>
            {
                if (!await userService.Validate2FA(user.Id, Totp))
                {
                    errorMessageTimer.Stop();
                    errorMessageTimer.Start();
                    ExecUI(() =>
                    {
                        ErrorMessage = "Two-Factor authentication failed!";
                        Totp = string.Empty;
                    });
                }
                else
                {
                    ExecUI(() => eventAggregator.GetEvent<UserCreationVerifiedEvent>().Publish());
                }

                ExecUI(() => pendingAttempt = false);
            });
        }

        private void ExportFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            if (sender is SaveFileDialog dialog)
            {
                dialog.FileOk -= ExportFileDialog_FileOk;
                if (dialog.FileName.NullOrEmpty() || BackupCodes.Count == 0)
                {
                    return;
                }

                var sb = new StringBuilder(512);
                sb.AppendLine("Glitched Epistle 2FA Backup Codes\n");
                sb.AppendLine($"User: {user.Id}");
                sb.AppendLine($"Export timestamp: {DateTime.UtcNow:s} (UTC)\n");

                for (var i = 0; i < BackupCodes.Count; i++)
                {
                    sb.AppendLine(BackupCodes[i]);
                }

                File.WriteAllText(dialog.FileName, sb.ToString());
            }
        }
    }
}