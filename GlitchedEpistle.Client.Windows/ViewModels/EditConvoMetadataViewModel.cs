using System;
using System.Timers;
using System.Windows;
using System.Windows.Input;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class EditConvoMetadataViewModel : ViewModel, ICloseable
    {
        #region Constants
        private readonly User user;
        private readonly IConvoService convoService;
        private readonly Timer messageTimer = new Timer(7000) { AutoReset = true };
        #endregion

        #region Events
        public event EventHandler<EventArgs> RequestedClose;
        #endregion

        #region Commands
        public ICommand CancelCommand { get; }
        public ICommand SubmitCommand { get; }
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

        private bool isAdmin = false;
        public bool IsAdmin
        {
            get => isAdmin;
            set => Set(ref isAdmin, value);
        }
        #endregion

        public Convo Convo { get; set; }

        public EditConvoMetadataViewModel(IConvoService convoService, User user)
        {
            this.user = user;
            this.convoService = convoService;

            SubmitCommand = new DelegateCommand(OnSubmit);
            CancelCommand = new DelegateCommand(OnCancel);

            messageTimer.Elapsed += (_, __) => ResetMessages();
            messageTimer.Start();
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

            // TODO: submit here
        }
    }
}
