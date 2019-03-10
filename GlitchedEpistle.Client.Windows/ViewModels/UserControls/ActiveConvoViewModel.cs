using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

using GlitchedPolygons.Services.MethodQ;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Messages;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;

using Prism.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class ActiveConvoViewModel : ViewModel
    {
        #region Constants
        // Injections:
        private readonly User user;
        private readonly IMethodQ methodQ;
        private readonly ISettings settings;
        private readonly IUserService userService;
        private readonly IConvoService convoService;
        private readonly IConvoProvider convoProvider;
        private readonly IMessageCryptography crypto;
        private readonly IEventAggregator eventAggregator;
        #endregion

        #region Commands
        public ICommand SendTextCommand { get; }
        public ICommand SendFileCommand { get; }
        public ICommand CopyConvoIdToClipboardCommand { get; }
        #endregion

        #region UI Bindings
        private string text;
        public string Text { get => text; set => Set(ref text, value); }

        private Visibility clipboardTickVisibility = Visibility.Hidden;
        public Visibility ClipboardTickVisibility { get => clipboardTickVisibility; set => Set(ref clipboardTickVisibility, value); }

        private ObservableCollection<Message> messages;
        public ObservableCollection<Message> Messages { get => messages; set => Set(ref messages, value); }
        #endregion

        public Convo ActiveConvo { get; set; }
        private ulong? scheduledHideGreenTickIcon = null, scheduledUpdateRoutine = null;

        public ActiveConvoViewModel(User user, IConvoService convoService, IConvoProvider convoProvider, IEventAggregator eventAggregator, IMethodQ methodQ, IUserService userService, IMessageCryptography crypto, ISettings settings)
        {
            this.user = user;
            this.crypto = crypto;
            this.methodQ = methodQ;
            this.settings = settings;
            this.userService = userService;
            this.convoService = convoService;
            this.convoProvider = convoProvider;
            this.eventAggregator = eventAggregator;

            SendTextCommand = new DelegateCommand(OnSendText);
            SendFileCommand = new DelegateCommand(OnSendFile);
            CopyConvoIdToClipboardCommand = new DelegateCommand(OnClickedCopyConvoIdToClipboard);

            settings.Load();

            scheduledUpdateRoutine = methodQ.Schedule(PullNewestMessages, TimeSpan.FromMilliseconds(500));
        }

        ~ActiveConvoViewModel()
        {
            if (scheduledUpdateRoutine.HasValue)
                methodQ?.Cancel(scheduledUpdateRoutine.Value);
        }

        private async Task<List<Tuple<string, string>>> GetKeys()
        {
            var stringBuilder = new StringBuilder(100);
            int participantsCount = ActiveConvo.Participants.Count;
            for (int i = 0; i < participantsCount; i++)
            {
                stringBuilder.Append(ActiveConvo.Participants[i]);
                if (i < participantsCount - 1)
                {
                    stringBuilder.Append(',');
                }
            }
            return await userService.GetUserPublicKeyXml(user.Id, stringBuilder.ToString(), user.Token.Item2);
        }

        private void PullNewestMessages()
        {
            // TODO: get newest msgs here
        }

        private async void OnSendText(object commandParam)
        {
            if (string.IsNullOrEmpty(Text))
            {
                return;
            }

            string username = settings["Username"];

            JObject json = new JObject();
            JObject messageJson = new JObject { ["u"] = user.Id, ["n"] = username, ["t"] = Text };
            foreach (Tuple<string, string> key in await GetKeys())
            {
                if (key is null || string.IsNullOrEmpty(key.Item1) || string.IsNullOrEmpty(key.Item2))
                {
                    continue;
                }

                json[key.Item1] = crypto.EncryptMessage(messageJson.ToString(Formatting.None), RSAParametersExtensions.FromXml(key.Item2));
            }

            bool success = await convoService.PostMessage(
                userId: user.Id,
                senderName: username,
                auth: user.Token.Item2,
                convoId: ActiveConvo.Id,
                convoPasswordHash: ActiveConvo.PasswordSHA512,
                messageBodiesJson: json.ToString(Formatting.None)
            );

            // TODO: add message to chatroom here

            if (!success)
            {
                var errorView = new InfoDialogView { DataContext = new InfoDialogViewModel { OkButtonText = "Okay :/", Text = "ERROR: Your message couldn't be uploaded to the epi", Title = "Message upload failed" } };
                errorView.ShowDialog();
            }
            else
            {
                Text = null;
            }
        }

        private void OnSendFile(object commandParam)
        {
            // TODO: Open file dialog here and then encode, encrypt and package message and prepare for submission
            throw new NotImplementedException();
        }

        private void OnClickedCopyConvoIdToClipboard(object commandParam)
        {
            Clipboard.SetText(ActiveConvo.Id);
            ClipboardTickVisibility = Visibility.Visible;

            if (scheduledHideGreenTickIcon.HasValue)
                methodQ.Cancel(scheduledHideGreenTickIcon.Value);

            scheduledHideGreenTickIcon = methodQ.Schedule(HideGreenTick, DateTime.UtcNow.AddSeconds(3));
        }

        private void HideGreenTick()
        {
            ClipboardTickVisibility = Visibility.Hidden;
            scheduledHideGreenTickIcon = null;
        }
    }
}
