using System;
using System.IO;
using System.Net;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;

using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class EditConvoMetadataViewModel : ViewModel, ICloseable
    {
        #region Constants
        private readonly User user;
        private readonly IUserService userService;
        private readonly IConvoService convoService;
        private readonly IConvoProvider convoProvider;
        private readonly IEventAggregator eventAggregator;
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
        public ICommand DeleteLocallyCommand { get; }
        public ICommand OldPasswordChangedCommand { get; }
        public ICommand NewPasswordChangedCommand { get; }
        public ICommand NewPassword2ChangedCommand { get; }
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

        private string name;
        public string Name
        {
            get => name;
            set => Set(ref name, value);
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
                }
            }
        }

        private string oldPw, newPw, newPw2;

        public EditConvoMetadataViewModel(IConvoService convoService, User user, IUserService userService, IConvoProvider convoProvider, IEventAggregator eventAggregator)
        {
            this.user = user;
            this.userService = userService;
            this.convoService = convoService;
            this.convoProvider = convoProvider;
            this.eventAggregator = eventAggregator;

            LeaveCommand = new DelegateCommand(OnLeave);
            SubmitCommand = new DelegateCommand(OnSubmit);
            CancelCommand = new DelegateCommand(OnCancel);
            DeleteCommand = new DelegateCommand(OnDelete);
            DeleteLocallyCommand=new DelegateCommand(OnDeleteLocally);
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
            Application.Current?.Dispatcher?.Invoke(() =>
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

        private void DeleteConvoLocally()
        {
            var jsonFile = new FileInfo(Path.Combine(Paths.CONVOS_DIRECTORY, Convo.Id + ".json"));
            if (jsonFile.Exists)
            {
                jsonFile.Delete();
            }

            var dir = new DirectoryInfo(Path.Combine(Paths.CONVOS_DIRECTORY, Convo.Id));
            if (dir.Exists)
            {
                dir.DeleteRecursively();
                dir.Delete();
            }
        }

        private void OnCancel(object commandParam)
        {
            RequestedClose?.Invoke(null, EventArgs.Empty);
        }

        private void OnLeave(object commandParam)
        {
            if (oldPw.NullOrEmpty())
            {
                PrintMessage("Please authenticate your request by providing the current convo's password.", true);
                return;
            }

            bool? confirmed = new ConfirmLeaveConvoView().ShowDialog();
            if (confirmed.HasValue && confirmed.Value == true)
            {
                Task.Run(async () =>
                {
                    bool success = await convoService.LeaveConvo(Convo.Id, oldPw.SHA512(), user.Id, user.Token.Item2);
                    if (!success)
                    {
                        PrintMessage("The request could not be fulfilled server-side; please double check the provided password and make sure that you actually are currently a participant of this convo.", true);
                        return;
                    }

                    DeleteConvoLocally();

                    PrintMessage($"You left the convo \"{Convo.Name}\" successfully! You are no longer a participant of it and can now close this window.", false);
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        UIEnabled = false;
                        eventAggregator.GetEvent<DeletedConvoEvent>().Publish(Convo.Id);
                    });
                });
            }
        }

        private void OnDeleteLocally(object commandParam)
        {
            var dialog = new ConfirmDeleteConvoLocallyView();
            bool? confirmed = dialog.ShowDialog();
            if (confirmed.HasValue && confirmed.Value == true)
            {
                Task.Run(() =>
                {
                    DeleteConvoLocally();

                    PrintMessage("Convo deleted successfully, it's gone... You can now close this window.", false);
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        UIEnabled = false;
                        eventAggregator.GetEvent<DeletedConvoEvent>().Publish(Convo.Id);
                    });
                });
            }
        }

        private void OnDelete(object commandParam)
        {
            CanSubmit = false;

            if (oldPw.NullOrEmpty())
            {
                PrintMessage("Please authenticate your request by providing the current convo's password.", true);
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
                    bool success = await convoService.DeleteConvo(Convo.Id, oldPw.SHA512(), user.Id, user.Token.Item2);
                    if (!success)
                    {
                        PrintMessage("The convo deletion request could not be fulfilled server-side; please double check the provided password and make sure that you are actually the convo's admin.", true);
                        Application.Current?.Dispatcher?.Invoke(() => CanSubmit = true);
                        return;
                    }

                    DeleteConvoLocally();

                    PrintMessage("Convo deleted successfully, it's gone... You can now close this window.", false);
                    Application.Current?.Dispatcher?.Invoke(() =>
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

            string totp = commandParam as string;

            if (totp.NullOrEmpty())
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
                bool totpValid = await userService.Validate2FA(user.Id, totp);

                if (!totpValid)
                {
                    PrintMessage("Two-Factor Authentication failed! Convo creation request rejected.", true);
                    Application.Current?.Dispatcher?.Invoke(() => CanSubmit = true);
                    return;
                }

                var dto = new ConvoChangeMetadataDto();

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
                    dto.PasswordSHA512 = newPw.SHA512();
                }

                bool successful = await convoService.ChangeConvoMetadata(Convo.Id, oldPw, user.Id, user.Token.Item2, dto);

                if (!successful)
                {
                    PrintMessage("Convo metadata change request was rejected. Perhaps you provided the wrong password or invalid data? Please double check the form.", true);
                    Application.Current?.Dispatcher?.Invoke(() => CanSubmit = true);
                    return;
                }

                PrintMessage("Convo metadata changed successfully. You can now close this window.", false);

                // TODO: save out convo here!

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    UIEnabled = false;
                    eventAggregator.GetEvent<ChangedConvoMetadataEvent>().Publish(Convo.Id);
                });
            });
        }
    }
}
