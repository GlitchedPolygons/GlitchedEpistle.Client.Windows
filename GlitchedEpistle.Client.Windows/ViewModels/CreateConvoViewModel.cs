using System;
using System.IO;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;

using GlitchedPolygons.Services.MethodQ;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;

using Prism.Events;
using Newtonsoft.Json;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class CreateConvoViewModel : ViewModel, ICloseable
    {
        #region Constants
        // Injections:
        private readonly User user;
        private readonly ILogger logger;
        private readonly IMethodQ methodQ;
        private readonly IUserService userService;
        private readonly IConvoService convoService;
        private readonly IConvoProvider convoProvider;
        private readonly IEventAggregator eventAggregator;
        #endregion

        #region Events
        public event EventHandler<EventArgs> RequestedClose;
        #endregion

        #region Commands
        public ICommand ClosedCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SubmitCommand { get; }
        public ICommand PasswordChangedCommand { get; }
        public ICommand Password2ChangedCommand { get; }
        #endregion

        #region UI Bindings
        private string name;
        public string Name { get => name; set => Set(ref name, value); }

        private string description;
        public string Description { get => description; set => Set(ref description, value); }

        private DateTime minExpirationUTC = DateTime.UtcNow.AddDays(2);
        public DateTime MinExpirationUTC { get => minExpirationUTC; set => Set(ref minExpirationUTC, value); }

        private DateTime expirationUTC = DateTime.UtcNow.AddDays(14);
        public DateTime ExpirationUTC { get => expirationUTC; set => Set(ref expirationUTC, value); }

        private string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; set => Set(ref errorMessage, value); }

        private string successMessage = string.Empty;
        public string SuccessMessage { get => successMessage; set => Set(ref successMessage, value); }

        private bool canSubmit = true;
        public bool CanSubmit { get => canSubmit; set => Set(ref canSubmit, value); }
        #endregion

        private string pw = string.Empty;
        private string pw2 = string.Empty;
        private readonly List<ulong> scheduledActions = new List<ulong>(6);

        public CreateConvoViewModel(User user, ILogger logger, IMethodQ methodQ, IEventAggregator eventAggregator, IUserService userService, IConvoService convoService, IConvoProvider convoProvider)
        {
            this.user = user;
            this.logger = logger;
            this.methodQ = methodQ;
            this.userService = userService;
            this.convoService = convoService;
            this.convoProvider = convoProvider;
            this.eventAggregator = eventAggregator;

            SubmitCommand = new DelegateCommand(OnSubmit);
            CancelCommand = new DelegateCommand(OnClickedCancel);
            PasswordChangedCommand = new DelegateCommand(pwBox => pw = (pwBox as PasswordBox)?.Password);
            Password2ChangedCommand = new DelegateCommand(pwBox => pw2 = (pwBox as PasswordBox)?.Password);
            ClosedCommand = new DelegateCommand(o => { for (int i = 0; i < scheduledActions.Count; i++) methodQ?.Cancel(scheduledActions[i]); });

            scheduledActions.Add(methodQ.Schedule(ResetMessages, TimeSpan.FromSeconds(7)));
        }

        ~CreateConvoViewModel()
        {
            for (int i = 0; i < scheduledActions.Count; i++)
                methodQ?.Cancel(scheduledActions[i]);
        }

        private void OnClickedCancel(object commandParam)
        {
            RequestedClose?.Invoke(null, EventArgs.Empty);
        }

        private void ResetMessages() => ErrorMessage = SuccessMessage = null;

        private async void OnSubmit(object commandParam)
        {
            string totp = commandParam as string;

            if (string.IsNullOrEmpty(totp))
            {
                ResetMessages();
                ErrorMessage = "No 2FA token provided - please take security seriously and authenticate your request!";
                return;
            }

            bool totpValid = await userService.Validate2FA(user.Id, totp);

            if (!totpValid)
            {
                ResetMessages();
                ErrorMessage = "Two-Factor Authentication failed! Convo creation request rejected.";
                return;
            }

            if (pw != pw2)
            {
                ResetMessages();
                ErrorMessage = "The password does not match its confirmation; please make sure that you re-type your password correctly!";
                return;
            }

            if (pw.Length < 5)
            {
                ResetMessages();
                ErrorMessage = "Your password is too weak; make sure that it has at least >5 characters!";
                return;
            }

            var convoCreationDto = new ConvoCreationDto
            {
                Name = Name,
                Description = Description,
                Expires = ExpirationUTC,
                PasswordSHA512 = pw.SHA512()
            };

            string id = await convoService.CreateConvo(convoCreationDto, user.Id, user.Token.Item2);

            if (!string.IsNullOrEmpty(id))
            {
                // Create the convo model object and feed it into the convo provider.
                var convo = new Convo
                {
                    Id = id,
                    CreatorId = user.Id,
                    CreationTimestamp = DateTime.UtcNow,
                    Name = Name,
                    Description = Description,
                    Expires = ExpirationUTC,
                    Participants = new List<string> { user.Id }
                };

                convoProvider.Convos.Add(convo);

                // Prepare the convo metadata file + messages directory.
                Directory.CreateDirectory(Path.Combine(Paths.CONVOS_DIRECTORY, id));
                File.WriteAllText(Path.Combine(Paths.CONVOS_DIRECTORY, id + ".json"), JsonConvert.SerializeObject(convo, Formatting.Indented));

                // Raise the convo created event application-wide (the main view will subscribe to this to update its list).
                eventAggregator.GetEvent<ConvoCreationSucceededEvent>().Publish(id);

                logger.LogMessage($"Convo {Name} created successfully under the id {id}.");

                // Display success message and close the window automatically after 5s.
                if (methodQ.Cancel(scheduledActions[0]))
                    scheduledActions.RemoveAt(0);

                ResetMessages();
                CanSubmit = false;
                SuccessMessage = $"The convo {Name} has been created successfully under the id {id}. You can close this window now.";
            }
            else
            {
                logger.LogError($"Convo {Name} couldn't be created. Reason unknown.");
            }
        }
    }
}
