using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class EditConvoMetadataViewModel : ViewModel, ICloseable
    {
        #region Constants
        private readonly User user;
        private readonly IUserService userService;
        private readonly IConvoService convoService;
        private readonly Timer messageTimer = new Timer(7000) { AutoReset = true };
        #endregion

        #region Events
        public event EventHandler<EventArgs> RequestedClose;
        #endregion

        #region Commands
        public ICommand CancelCommand { get; }
        public ICommand SubmitCommand { get; }
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

        public EditConvoMetadataViewModel(IConvoService convoService, User user, IUserService userService)
        {
            this.user = user;
            this.userService = userService;
            this.convoService = convoService;

            SubmitCommand = new DelegateCommand(OnSubmit);
            CancelCommand = new DelegateCommand(OnCancel);
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

        private void OnCancel(object commandParam)
        {
            RequestedClose?.Invoke(null, EventArgs.Empty);
        }

        private void OnSubmit(object commandParam)
        {
            string totp = commandParam as string;

            if (totp.NullOrEmpty())
            {
                PrintMessage("No 2FA token provided - please take security seriously and authenticate your request!", true);
                return;
            }

            CanSubmit = false;

            if (newPw != newPw2)
            {
                PrintMessage("The new password does not match its confirmation; please make sure that you re-type your password correctly!", true);
                CanSubmit = true;
                return;
            }

            if (newPw.Length < 5)
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
                
                // TODO: submit here
            });
        }
    }
}
