using System;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using System.Windows.Controls;

using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using Prism.Events;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels
{
    public class JoinConvoDialogViewModel : ViewModel, ICloseable
    {
        #region Constants
        private readonly Timer messageTimer = new Timer(7000) { AutoReset = true };

        // Injections:
        private readonly User user;
        private readonly IConvoService convoService;
        private readonly IConvoProvider convoProvider;
        private readonly IEventAggregator eventAggregator;
        #endregion

        #region Events
        public event EventHandler<EventArgs> RequestedClose;
        #endregion

        #region Commands
        public ICommand JoinCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        #region UI Bindings
        private string errorMessage = string.Empty;
        public string ErrorMessage { get => errorMessage; set => Set(ref errorMessage, value); }

        private string convoId = string.Empty;
        public string ConvoId { get => convoId; set => Set(ref convoId, value); }
        #endregion

        public JoinConvoDialogViewModel(IConvoService convoService, IEventAggregator eventAggregator, IConvoProvider convoProvider, User user)
        {
            this.user = user;
            this.convoService = convoService;
            this.convoProvider = convoProvider;
            this.eventAggregator = eventAggregator;

            JoinCommand = new DelegateCommand(OnClickedJoinConvo);
            CancelCommand = new DelegateCommand(_ => RequestedClose?.Invoke(null, EventArgs.Empty));

            messageTimer.Elapsed += (_, __) => ErrorMessage = null;
            messageTimer.Start();
        }

        private void ResetMessages()
        {
            messageTimer.Stop();
            messageTimer.Start();
            ErrorMessage = null;
        }

        private async void OnClickedJoinConvo(object commandParam)
        {
            if (commandParam is PasswordBox passwordBox)
            {
                string pw = passwordBox.Password.SHA512();
                if (!await convoService.JoinConvo(ConvoId, pw, user.Id, user.Token.Item2))
                {
                    ResetMessages();
                    ErrorMessage = "ERROR: Couldn't join convo. Please double check the credentials and try again.";
                    return;
                }

                var convo = convoProvider[ConvoId];
                if (convo is null)
                {
                    var metadata = await convoService.GetConvoMetadata(ConvoId, pw, user.Id, user.Token.Item2);
                    if (metadata != null)
                    {
                        convo = new Convo
                        {
                            Id = metadata.Id,
                            Name = metadata.Name,
                            Expires = metadata.Expires,
                            CreatorId = metadata.CreatorId,
                            Description = metadata.Description,
                            CreationTimestamp = metadata.CreationTimestamp,
                            BannedUsers = metadata.BannedUsers.Split(',').ToList(),
                            Participants = metadata.Participants.Split(',').ToList(),
                            PasswordSHA512 = pw
                        };
                        convoProvider.Convos.Add(convo);
                    }
                }

                eventAggregator.GetEvent<JoinedConvoEvent>().Publish(convo);
            }
        }
    }
}
