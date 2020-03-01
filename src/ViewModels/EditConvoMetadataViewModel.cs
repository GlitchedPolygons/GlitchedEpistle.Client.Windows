/*
    Glitched Epistle - Windows Client
    Copyright (C) 2020 Raphael Beck

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
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Text.Json;

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.RepositoryPattern;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.Convos;
using GlitchedPolygons.Services.CompressionUtility;
using GlitchedPolygons.Services.Cryptography.Asymmetric;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class EditConvoMetadataViewModel : ViewModel, ICloseable
    {
        #region Constants
        private readonly User user;
        private readonly IConvoService convoService;
        private readonly IEventAggregator eventAggregator;
        private readonly IAsymmetricCryptographyRSA crypto;
        private readonly ICompressionUtility compressionUtility;
        private readonly IConvoPasswordProvider convoPasswordProvider;
        #endregion

        #region Events
        public event EventHandler<EventArgs> RequestedClose;
        #endregion

        #region Commands
        public ICommand LeaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SubmitCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand OldPasswordChangedCommand { get; }
        public ICommand NewPasswordChangedCommand { get; }
        public ICommand NewPassword2ChangedCommand { get; }
        public ICommand MakeAdminCommand { get; }
        public ICommand KickAndBanUserCommand { get; }

        public ICommand CopyUserIdToClipboardCommand
        {
            get => new DelegateCommand(commandParam =>
            {
                if (commandParam is string userId)
                {
                    Clipboard.SetText(userId);

                    new InfoDialogView
                    {
                        DataContext = new InfoDialogViewModel
                        {
                            MaxWidth = 350,
                            Title = "Success",
                            OkButtonText = "Cool",
                            Text = $"The user id \"{userId}\" has been copied successfully to the clipboard!"
                        }
                    }.ShowDialog();
                }
            });
        }
        #endregion

        #region UI Bindings
        public string UserId => user?.Id;

        private string name;
        public string Name
        {
            get => name;
            set => Set(ref name, value);
        }

        private string totp;
        public string Totp
        {
            get => totp;
            set => Set(ref totp, value);
        }

        private string description;
        public string Description
        {
            get => description;
            set => Set(ref description, value);
        }

        private DateTime minExpirationUTC = DateTime.UtcNow.AddDays(2);
        public DateTime MinExpirationUTC
        {
            get => minExpirationUTC;
            set => Set(ref minExpirationUTC, value);
        }

        private DateTime expirationUTC = DateTime.UtcNow.AddDays(14);
        public DateTime ExpirationUTC
        {
            get => expirationUTC;
            set => Set(ref expirationUTC, value);
        }

        private bool uiEnabled = true;
        public bool UIEnabled { get => uiEnabled; set => Set(ref uiEnabled, value); }

        public bool IsAdmin => Convo?.CreatorId.Equals(user?.Id) ?? false;

        private bool canSubmit = true;
        public bool CanSubmit
        {
            get => canSubmit;
            set => Set(ref canSubmit, value);
        }

        public bool CanLeave => !IsAdmin;

        private ObservableCollection<string> participants = new ObservableCollection<string>();
        public ObservableCollection<string> Participants
        {
            get => participants;
            set => Set(ref participants, value);
        }

        public Visibility OtherParticipantsListVisibility => Participants.Count > 0 ? Visibility.Visible : Visibility.Hidden;

        private ObservableCollection<string> banned = new ObservableCollection<string>();
        public ObservableCollection<string> Banned
        {
            get => banned;
            set => Set(ref banned, value);
        }

        public Visibility BannedListVisibility => Banned.Count > 0 ? Visibility.Visible : Visibility.Hidden;
        #endregion

        private Convo convo;
        public Convo Convo
        {
            get => convo;
            set
            {
                if ((convo = value) != null)
                {
                    Name = convo.Name;
                    Description = convo.Description;
                    ExpirationUTC = convo.ExpirationUTC.FromUnixTimeSeconds();
                    RefreshParticipantLists();
                }
            }
        }

        private readonly IRepository<Convo, string> convoProvider;

        private void RefreshParticipantLists()
        {
            Participants = new ObservableCollection<string>();
            foreach (string userId in convo.Participants)
            {
                if (userId.Equals(this.user.Id))
                    continue;

                Participants.Add(userId);
            }

            Banned = new ObservableCollection<string>();
            foreach (string bannedUserId in convo.BannedUsers)
            {
                if (bannedUserId.NullOrEmpty())
                    continue;

                Banned.Add(bannedUserId);
            }
        }

        private string oldPw, newPw, newPw2;

        public EditConvoMetadataViewModel(IConvoService convoService, User user, IEventAggregator eventAggregator, IConvoPasswordProvider convoPasswordProvider, IAsymmetricCryptographyRSA crypto, ICompressionUtility compressionUtility)
        {
            this.user = user;
            this.compressionUtility = compressionUtility;
            this.crypto = crypto;
            this.convoService = convoService;
            this.eventAggregator = eventAggregator;
            this.convoPasswordProvider = convoPasswordProvider;

            convoProvider = new ConvoRepositorySQLite($"Data Source={Path.Combine(Paths.GetConvosDirectory(user.Id), "_metadata.db")};Version=3;");

            LeaveCommand = new DelegateCommand(OnLeave);
            SubmitCommand = new DelegateCommand(OnSubmit);
            CancelCommand = new DelegateCommand(OnCancel);
            DeleteCommand = new DelegateCommand(OnDelete);
            MakeAdminCommand = new DelegateCommand(OnMakeAdmin);
            KickAndBanUserCommand = new DelegateCommand(OnKickAndBanUser);
            OldPasswordChangedCommand = new DelegateCommand(pwBox => oldPw = (pwBox as PasswordBox)?.Password);
            NewPasswordChangedCommand = new DelegateCommand(pwBox => newPw = (pwBox as PasswordBox)?.Password);
            NewPassword2ChangedCommand = new DelegateCommand(pwBox => newPw2 = (pwBox as PasswordBox)?.Password);
        }

        ~EditConvoMetadataViewModel()
        {
            oldPw = newPw = newPw2 = null;
        }

        private async Task DeleteConvoLocally()
        {
            var sqliteDbFile = new FileInfo(Path.Combine(Paths.GetConvosDirectory(user.Id), Convo.Id + ".db"));
            if (sqliteDbFile.Exists)
            {
                sqliteDbFile.Delete();
            }

            var dir = new DirectoryInfo(Path.Combine(Paths.GetConvosDirectory(user.Id), Convo.Id));
            if (dir.Exists)
            {
                dir.DeleteRecursively();
                dir.Delete();
            }

            await convoProvider.Remove(Convo);
        }

        private void OnCancel(object commandParam)
        {
            RequestedClose?.Invoke(null, EventArgs.Empty);
        }

        private void OnLeave(object commandParam)
        {
            if (Totp.NullOrEmpty())
            {
                ErrorMessage = "Please authenticate your request by providing your two-factor authentication token.";
                return;
            }

            bool? confirmed = new ConfirmLeaveConvoView().ShowDialog();

            if (confirmed.HasValue && confirmed.Value == true)
            {
                Task.Run(async () =>
                {
                    var dto = new ConvoLeaveRequestDto
                    {
                        ConvoId = Convo.Id,
                        Totp = Totp
                    };

                    var body = new EpistleRequestBody
                    {
                        UserId = user.Id,
                        Auth = user.Token.Item2,
                        Body = JsonSerializer.Serialize(dto)
                    };

                    bool success = await convoService.LeaveConvo(body.Sign(crypto, user.PrivateKeyPem));

                    if (!success)
                    {
                        ErrorMessage = "The request could not be fulfilled server-side; please double check the provided password and make sure that you actually are currently a participant of this convo.";
                        return;
                    }

                    await DeleteConvoLocally();

                    SuccessMessage = $"You left the convo \"{Convo.Name}\" successfully! You are no longer a participant of it and can now close this window.";
                    UIEnabled = false;

                    ExecUI(() => eventAggregator.GetEvent<DeletedConvoEvent>().Publish(Convo.Id));
                });
            }
        }

        private void OnMakeAdmin(object commandParam)
        {
            if (oldPw.NullOrEmpty())
            {
                ErrorMessage = "Please authenticate your request by providing the current convo's password (at the top of the form).";
                return;
            }

            if (Totp.NullOrEmpty())
            {
                ErrorMessage = "Please authenticate your request by providing your two-factor authentication token.";
                return;
            }

            if (commandParam is string newAdminUserId)
            {
                bool? confirmed = new ConfirmChangeConvoAdminView().ShowDialog();
                if (confirmed.HasValue && confirmed.Value == true)
                {
                    Task.Run(async () =>
                    {
                        var dto = new ConvoChangeMetadataRequestDto
                        {
                            Totp = Totp,
                            ConvoId = Convo.Id,
                            ConvoPasswordSHA512 = oldPw.SHA512(),
                            NewConvoPasswordSHA512 = null,
                            Name = null,
                            Description = null,
                            ExpirationUTC = null,
                            CreatorId = newAdminUserId
                        };

                        var body = new EpistleRequestBody
                        {
                            UserId = user.Id,
                            Auth = user.Token.Item2,
                            Body = compressionUtility.Compress(JsonSerializer.Serialize(dto))
                        };

                        bool success = await convoService.ChangeConvoMetadata(body.Sign(crypto, user.PrivateKeyPem));
                        if (!success)
                        {
                            ErrorMessage = "The convo admin change request was rejected server-side! Perhaps double-check the provided convo password?";
                            return;
                        }

                        var convo = await convoProvider.Get(Convo.Id);
                        if (convo != null)
                        {
                            convo.CreatorId = newAdminUserId;

                            if (await convoProvider.Update(convo))
                            {
                                SuccessMessage = $"Success! The user {newAdminUserId} is now the new convo admin. You can now close this window...";
                            }
                            else
                            {
                                ErrorMessage = "The convo admin change request was accepted server-side but couldn't be applied locally. Try re-syncing the convo...";
                            }
                        }

                        ExecUI(() =>
                        {
                            UIEnabled = false;
                            eventAggregator.GetEvent<ChangedConvoMetadataEvent>().Publish(Convo.Id);
                        });
                    });
                }
            }
        }

        private void OnKickAndBanUser(object commandParam)
        {
            if (oldPw.NullOrEmpty())
            {
                ErrorMessage = "Please authenticate your request by providing the current convo's password (at the top of the form).";
                return;
            }

            if (Totp.NullOrEmpty())
            {
                ErrorMessage = "Please authenticate your request by providing your two-factor authentication token.";
                return;
            }

            if (commandParam is string userIdToKick)
            {
                bool? confirmed = new ConfirmKickUserView().ShowDialog();
                if (confirmed.HasValue && confirmed.Value == true)
                {
                    Task.Run(async () =>
                    {
                        var dto = new ConvoKickUserRequestDto
                        {
                            ConvoId = Convo.Id,
                            ConvoPasswordSHA512 = oldPw.SHA512(),
                            UserIdToKick = userIdToKick,
                            PermaBan = true,
                            Totp = Totp
                        };

                        var body = new EpistleRequestBody
                        {
                            UserId = user.Id,
                            Auth = user.Token.Item2,
                            Body = compressionUtility.Compress(JsonSerializer.Serialize(dto))
                        };

                        bool success = await convoService.KickUser(body.Sign(crypto, user.PrivateKeyPem));
                        if (!success)
                        {
                            ErrorMessage = $"The user \"{userIdToKick}\" could not be kicked and banned from the convo. Perhaps double-check the provided convo password?";
                            return;
                        }

                        SuccessMessage = $"The user \"{userIdToKick}\" has been kicked out of the convo.";

                        var convo = await convoProvider.Get(Convo.Id);
                        if (convo != null)
                        {
                            convo.BannedUsers.Add(userIdToKick);
                            convo.Participants.Remove(userIdToKick);

                            await convoProvider.Update(convo);
                        }

                        RefreshParticipantLists();
                        ExecUI(() => eventAggregator.GetEvent<ChangedConvoMetadataEvent>().Publish(Convo.Id));
                    });
                }
            }
        }

        private void OnDelete(object commandParam)
        {
            CanSubmit = false;

            if (Totp.NullOrEmpty())
            {
                ErrorMessage = "Please authenticate your request by providing your two-factor authentication token.";
                CanSubmit = true;
                return;
            }

            var dialog = new ConfirmConvoDeletionView();
            dialog.ConvoIdLabel.Content = Convo.Id;
            dialog.ConvoNameLabel.Content = Convo.Name;

            bool? confirmed = dialog.ShowDialog();
            if (confirmed.HasValue && confirmed.Value == true)
            {
                Task.Run(async () =>
                {
                    var dto = new ConvoDeletionRequestDto
                    {
                        ConvoId = Convo.Id,
                        Totp = Totp
                    };

                    var body = new EpistleRequestBody
                    {
                        UserId = user.Id,
                        Auth = user.Token.Item2,
                        Body = JsonSerializer.Serialize(dto)
                    };

                    bool success = await convoService.DeleteConvo(body.Sign(crypto, user.PrivateKeyPem));
                    if (!success)
                    {
                        ErrorMessage = "The convo deletion request could not be fulfilled server-side; please double check the provided 2FA token and try again.";
                        ExecUI(() => CanSubmit = true);
                        return;
                    }

                    await DeleteConvoLocally();

                    SuccessMessage = "Convo deleted successfully, it's gone... You can now close this window.";
                    ExecUI(() =>
                    {
                        UIEnabled = false;
                        eventAggregator.GetEvent<DeletedConvoEvent>().Publish(Convo.Id);
                    });
                });
            }
        }

        private void OnSubmit(object commandParam)
        {
            CanSubmit = false;

            Task.Run(async () =>
            {
                if (oldPw.NullOrEmpty())
                {
                    ErrorMessage = "Please authenticate your request by providing the current convo's password.";
                    CanSubmit = true;
                    return;
                }

                if (Totp.NullOrEmpty())
                {
                    ErrorMessage = "No 2FA token provided - please take security seriously and authenticate your request!";
                    CanSubmit = true;
                    return;
                }

                if (newPw != newPw2)
                {
                    ErrorMessage = "The new password does not match its confirmation; please make sure that you re-type your password correctly!";
                    CanSubmit = true;
                    return;
                }

                if (newPw.NotNullNotEmpty() && newPw.Length < 5)
                {
                    ErrorMessage = "Your new password is too weak; make sure that it has at least >5 characters!";
                    CanSubmit = true;
                    return;
                }

                var dto = new ConvoChangeMetadataRequestDto
                {
                    Totp = Totp,
                    ConvoId = Convo.Id,
                    ConvoPasswordSHA512 = oldPw.SHA512(),
                };

                if (Name.NotNullNotEmpty() && Name != Convo.Name)
                {
                    dto.Name = Name;
                }

                if (Description.NotNullNotEmpty() && Description != Convo.Description)
                {
                    dto.Description = Description;
                }

                if (ExpirationUTC != Convo.ExpirationUTC.FromUnixTimeSeconds())
                {
                    dto.ExpirationUTC = ExpirationUTC.ToUnixTimeSeconds();
                }

                if (newPw.NotNullNotEmpty())
                {
                    dto.NewConvoPasswordSHA512 = newPw.SHA512();
                }

                var body = new EpistleRequestBody
                {
                    UserId = user.Id,
                    Auth = user.Token.Item2,
                    Body = compressionUtility.Compress(JsonSerializer.Serialize(dto))
                };

                bool successful = await convoService.ChangeConvoMetadata(body.Sign(crypto, user.PrivateKeyPem));

                if (!successful)
                {
                    ErrorMessage = "Convo metadata change request was rejected. Perhaps you provided the wrong password or invalid data? Please double check the form.";
                    ExecUI(() => CanSubmit = true);
                    return;
                }

                SuccessMessage = "Convo metadata changed successfully. You can now close this window.";

                var convo = await convoProvider.Get(Convo.Id);
                if (convo != null)
                {
                    if (dto.Name.NotNullNotEmpty())
                    {
                        convo.Name = dto.Name;
                    }
                    if (dto.Description.NotNullNotEmpty())
                    {
                        convo.Description = dto.Description;
                    }
                    if (dto.ExpirationUTC.HasValue)
                    {
                        convo.ExpirationUTC = dto.ExpirationUTC.Value;
                    }
                    if (dto.NewConvoPasswordSHA512.NotNullNotEmpty())
                    {
                        convoPasswordProvider.SetPasswordSHA512(convo.Id, dto.NewConvoPasswordSHA512);
                    }
                    await convoProvider.Update(convo);
                }

                UIEnabled = false;
                ExecUI(() => eventAggregator.GetEvent<ChangedConvoMetadataEvent>().Publish(Convo.Id));
            });
        }
    }
}
