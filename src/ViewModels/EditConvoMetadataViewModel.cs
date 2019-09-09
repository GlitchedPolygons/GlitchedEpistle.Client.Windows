/*
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
using System.IO;
using System.Net;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.RepositoryPattern;
using GlitchedPolygons.Services.CompressionUtility;
using GlitchedPolygons.Services.Cryptography.Asymmetric;

using Newtonsoft.Json;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class EditConvoMetadataViewModel : ViewModel, ICloseable
    {
        #region Constants
        private readonly User user;
        private readonly ICompressionUtility gzip;
        private readonly IUserService userService;
        private readonly IConvoService convoService;
        private readonly IEventAggregator eventAggregator;
        private readonly IAsymmetricCryptographyRSA crypto;
        private readonly IConvoPasswordProvider convoPasswordProvider;
        private readonly Timer messageTimer = new Timer(7000) { AutoReset = true };
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

                    var dialog = new InfoDialogView
                    {
                        DataContext = new InfoDialogViewModel
                        {
                            MaxWidth = 350,
                            Title = "Success",
                            OkButtonText = "Cool",
                            Text = $"The user id \"{userId}\" has been copied successfully to the clipboard!"
                        }
                    };

                    dialog.ShowDialog();
                }
            });
        }
        #endregion

        #region UI Bindings
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

        public bool IsAdmin
        {
            get
            {
                bool? isAdmin = Convo?.CreatorId.Equals(user?.Id);
                return isAdmin ?? false;
            }
        }

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
                convo = value;
                if (convo != null)
                {
                    Name = convo.Name;
                    Description = convo.Description;
                    ExpirationUTC = convo.ExpirationUTC;
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

        public EditConvoMetadataViewModel(IConvoService convoService, User user, IUserService userService, IEventAggregator eventAggregator, IConvoPasswordProvider convoPasswordProvider, IAsymmetricCryptographyRSA crypto, ICompressionUtility gzip)
        {
            this.user = user;
            this.gzip = gzip;
            this.crypto = crypto;
            this.userService = userService;
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

            messageTimer.Elapsed += (_, __) => ResetMessages();
            messageTimer.Start();
        }

        ~EditConvoMetadataViewModel()
        {
            oldPw = newPw = newPw2 = null;
        }

        private void ResetMessages()
        {
            messageTimer.Stop();
            messageTimer.Start();
            ErrorMessage = SuccessMessage = null;
        }

        private void PrintMessage(string message, bool error)
        {
            ExecUI(() =>
            {
                ResetMessages();

                if (error)
                {
                    ErrorMessage = message;
                }
                else
                {
                    SuccessMessage = message;
                }
            });
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
                PrintMessage("Please authenticate your request by providing your two-factor authentication token.", true);
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
                        Body = JsonConvert.SerializeObject(dto)
                    };

                    bool success = await convoService.LeaveConvo(body.Sign(crypto, user.PrivateKeyPem));

                    if (!success)
                    {
                        PrintMessage("The request could not be fulfilled server-side; please double check the provided password and make sure that you actually are currently a participant of this convo.", true);
                        return;
                    }

                    await DeleteConvoLocally();

                    PrintMessage($"You left the convo \"{Convo.Name}\" successfully! You are no longer a participant of it and can now close this window.", false);
                    ExecUI(() =>
                    {
                        UIEnabled = false;
                        eventAggregator.GetEvent<DeletedConvoEvent>().Publish(Convo.Id);
                    });
                });
            }
        }

        private void OnMakeAdmin(object commandParam)
        {
            if (oldPw.NullOrEmpty())
            {
                PrintMessage("Please authenticate your request by providing the current convo's password (at the top of the form).", true);
                return;
            }

            if (Totp.NullOrEmpty())
            {
                PrintMessage("Please authenticate your request by providing your two-factor authentication token.", true);
                return;
            }

            if (commandParam is string newAdminUserId)
            {
                bool? confirmed = new ConfirmChangeConvoAdminView().ShowDialog();
                if (confirmed.HasValue && confirmed.Value == true)
                {
                    Task.Run(async() =>
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
                            Body = gzip.Compress(JsonConvert.SerializeObject(dto))
                        };

                        bool success = await convoService.ChangeConvoMetadata(body.Sign(crypto, user.PrivateKeyPem));
                        if (!success)
                        {
                            PrintMessage("The convo admin change request was rejected server-side! Perhaps double-check the provided convo password?",true);
                            return;
                        }

                        var convo = await convoProvider.Get(Convo.Id);
                        if (convo != null)
                        {
                            convo.CreatorId = newAdminUserId;
                            
                            if (await convoProvider.Update(convo))
                            {
                                PrintMessage($"Success! The user {newAdminUserId} is now the new convo admin. You can now close this window...", false);
                            }
                            else
                            {
                                PrintMessage("The convo admin change request was accepted server-side but couldn't be applied locally. Try re-syncing the convo...", true);
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
                PrintMessage("Please authenticate your request by providing the current convo's password (at the top of the form).", true);
                return;
            }

            if (Totp.NullOrEmpty())
            {
                PrintMessage("Please authenticate your request by providing your two-factor authentication token.", true);
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
                            Body = gzip.Compress(JsonConvert.SerializeObject(dto))
                        };

                        bool success = await convoService.KickUser(body.Sign(crypto, user.PrivateKeyPem));
                        if (!success)
                        {
                            PrintMessage($"The user \"{userIdToKick}\" could not be kicked and banned from the convo. Perhaps double-check the provided convo password?", true);
                            return;
                        }

                        PrintMessage($"The user \"{userIdToKick}\" has been kicked out of the convo.", false);

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
                PrintMessage("Please authenticate your request by providing your two-factor authentication token.", true);
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
                        Body = JsonConvert.SerializeObject(dto)
                    };

                    bool success = await convoService.DeleteConvo(body.Sign(crypto, user.PrivateKeyPem));
                    if (!success)
                    {
                        PrintMessage("The convo deletion request could not be fulfilled server-side; please double check the provided 2FA token and try again.", true);
                        ExecUI(() => CanSubmit = true);
                        return;
                    }

                    await DeleteConvoLocally();
                    
                    PrintMessage("Convo deleted successfully, it's gone... You can now close this window.", false);
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

            if (oldPw.NullOrEmpty())
            {
                PrintMessage("Please authenticate your request by providing the current convo's password.", true);
                CanSubmit = true;
                return;
            }

            if (Totp.NullOrEmpty())
            {
                PrintMessage("No 2FA token provided - please take security seriously and authenticate your request!", true);
                CanSubmit = true;
                return;
            }

            if (newPw != newPw2)
            {
                PrintMessage("The new password does not match its confirmation; please make sure that you re-type your password correctly!", true);
                CanSubmit = true;
                return;
            }

            if (newPw.NotNullNotEmpty() && newPw.Length < 5)
            {
                PrintMessage("Your new password is too weak; make sure that it has at least >5 characters!", true);
                CanSubmit = true;
                return;
            }

            Task.Run(async () =>
            {
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

                if (ExpirationUTC != Convo.ExpirationUTC)
                {
                    dto.ExpirationUTC = ExpirationUTC;
                }

                if (newPw.NotNullNotEmpty())
                {
                    dto.NewConvoPasswordSHA512 = newPw.SHA512();
                }

                var body = new EpistleRequestBody
                {
                    UserId = user.Id,
                    Auth = user.Token.Item2,
                    Body = gzip.Compress(JsonConvert.SerializeObject(dto))
                };

                bool successful = await convoService.ChangeConvoMetadata(body.Sign(crypto, user.PrivateKeyPem));

                if (!successful)
                {
                    PrintMessage("Convo metadata change request was rejected. Perhaps you provided the wrong password or invalid data? Please double check the form.", true);
                    ExecUI(() => CanSubmit = true);
                    return;
                }

                PrintMessage("Convo metadata changed successfully. You can now close this window.", false);

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

                ExecUI(() =>
                {
                    UIEnabled = false;
                    eventAggregator.GetEvent<ChangedConvoMetadataEvent>().Publish(Convo.Id);
                });
            });
        }
    }
}
