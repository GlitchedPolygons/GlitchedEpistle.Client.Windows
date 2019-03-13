using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

using GlitchedPolygons.Services.MethodQ;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Messages;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;

using Prism.Events;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class ActiveConvoViewModel : ViewModel
    {
        #region Constants

        public const long MAX_FILE_SIZE_B = 20971520;
        public const string MSG_TIMESTAMP_FORMAT = "dd.MM.yyyy HH:mm";

        // Injections:
        private readonly User user;
        private readonly ILogger logger;
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

        public string Text
        {
            get => text;
            set => Set(ref text, value);
        }

        private Visibility clipboardTickVisibility = Visibility.Hidden;

        public Visibility ClipboardTickVisibility
        {
            get => clipboardTickVisibility;
            set => Set(ref clipboardTickVisibility, value);
        }

        private ObservableCollection<MessageViewModel> messages = new ObservableCollection<MessageViewModel>();

        public ObservableCollection<MessageViewModel> Messages
        {
            get => messages;
            set => Set(ref messages, value);
        }

        #endregion

        private Convo activeConvo;
        public Convo ActiveConvo
        {
            get => activeConvo;
            set { activeConvo = value; LoadLocalMessages(); }
        }

        private ulong? scheduledHideGreenTickIcon = null, scheduledUpdateRoutine = null;

        public ActiveConvoViewModel(User user, IConvoService convoService, IConvoProvider convoProvider, IEventAggregator eventAggregator, IMethodQ methodQ, IUserService userService, IMessageCryptography crypto, ISettings settings, ILogger logger)
        {
            this.user = user;
            this.logger = logger;
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

            scheduledUpdateRoutine = methodQ.Schedule(PullNewestMessages, TimeSpan.FromMilliseconds(500));

            settings.Load();
        }

        ~ActiveConvoViewModel()
        {
            if (scheduledUpdateRoutine.HasValue)
                methodQ?.Cancel(scheduledUpdateRoutine.Value);
        }

        private void LoadLocalMessages()
        {
            if (ActiveConvo is null || string.IsNullOrEmpty(ActiveConvo.Id))
                return;

            string dir = Path.Combine(Paths.CONVOS_DIRECTORY, ActiveConvo.Id);
            if (Directory.Exists(dir))
            {
                foreach (string file in Directory.GetFiles(dir).OrderBy(f => f))
                {
                    try
                    {
                        var message = JsonConvert.DeserializeObject<Message>(File.ReadAllText(file));
                        if (message is null)
                        {
                            continue;
                        }

                        AddMessageToView(message);
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"{nameof(ActiveConvoViewModel)}::{nameof(LoadLocalMessages)}: Failed to load message into convo chatroom view. Error message: {e}");
                    }
                }
            }
        }

        private void HideGreenTick()
        {
            ClipboardTickVisibility = Visibility.Hidden;
            scheduledHideGreenTickIcon = null;
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

        private async Task<bool> SubmitMessage(JObject messageBodyJson)
        {
            JObject messageBodiesJson = new JObject();

            foreach (Tuple<string, string> key in await GetKeys())
            {
                if (key is null || string.IsNullOrEmpty(key.Item1) || string.IsNullOrEmpty(key.Item2))
                {
                    continue;
                }

                messageBodiesJson[key.Item1] = crypto.EncryptMessage
                (
                    messageJson: messageBodyJson.ToString(Formatting.None),
                    recipientPublicRsaKey: RSAParametersExtensions.FromXml(key.Item2)
                );
            }

            JToken messageBody = messageBodiesJson[user.Id];
            if (messageBody is null)
            {
                return false;
            }

            string username = settings["Username"];

            var message = new Message
            {
                SenderId = user.Id,
                SenderName = username,
                Body = messageBody.ToString(),
                Timestamp = DateTime.UtcNow
            };

            AddMessageToView(message);

            File.WriteAllText
            (
                contents: JsonConvert.SerializeObject(message),
                path: Path.Combine(Paths.CONVOS_DIRECTORY, ActiveConvo.Id, DateTime.UtcNow.ToString("yyyyMMddHHmmssff"))
            );

            return await convoService.PostMessage
            (
                convoId: ActiveConvo.Id,
                messageDto: new PostMessageParamsDto
                {
                    UserId = user.Id,
                    SenderName = username,
                    Auth = user.Token.Item2,
                    ConvoPasswordHash = ActiveConvo.PasswordSHA512,
                    MessageBodiesJson = messageBodiesJson.ToString(Formatting.None)
                }
            );
        }

        private void AddMessageToView(Message message)
        {
            if (message is null)
            {
                return;
            }

            var messageViewModel = new MessageViewModel
            {
                SenderId = message.SenderId,
                SenderName = message.SenderName,
                Timestamp = message.Timestamp.ToLocalTime().ToString(MSG_TIMESTAMP_FORMAT),
            };

            var json = JToken.Parse(crypto.DecryptMessage(message.Body, user.PrivateKey));
            if (json is null)
            {
                return;
            }

            messageViewModel.Text = json["text"]?.Value<string>();
            messageViewModel.FileName = json["fileName"]?.Value<string>();
            string fileBase64 = json["fileBase64"]?.Value<string>();
            messageViewModel.FileBytes = string.IsNullOrEmpty(fileBase64) ? null : Convert.FromBase64String(fileBase64);

            Messages.Add(messageViewModel);
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

            JObject messageBodyJson = new JObject { ["text"] = Text };

            if (await SubmitMessage(messageBodyJson))
            {
                Text = null;
            }
            else
            {
                var errorView = new InfoDialogView { DataContext = new InfoDialogViewModel { OkButtonText = "Okay :/", Text = "ERROR: Your message couldn't be uploaded to the epistle Web API", Title = "Message upload failed" } };
                errorView.ShowDialog();
            }
        }

        private void OnSendFile(object commandParam)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Epistle - Select the file you want to send",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            dialog.FileOk += async (sender, e) =>
            {
                if (sender is OpenFileDialog _dialog && !string.IsNullOrEmpty(_dialog.FileName))
                {
                    byte[] file = File.ReadAllBytes(_dialog.FileName);

                    if (file.LongLength < MAX_FILE_SIZE_B)
                    {
                        var messageBodyJson = new JObject
                        {
                            ["fileName"] = Path.GetFileName(_dialog.FileName),
                            ["fileBase64"] = Convert.ToBase64String(file)
                        };

                        if (!await SubmitMessage(messageBodyJson))
                        {
                            var errorView = new InfoDialogView {DataContext = new InfoDialogViewModel {OkButtonText = "Okay :/", Text = "ERROR: Your file couldn't be uploaded to the epistle Web API", Title = "Message upload failed"}};
                            errorView.ShowDialog();
                        }
                    }
                    else
                    {
                        var errorView = new InfoDialogView { DataContext = new InfoDialogViewModel { OkButtonText = "Okay :/", Text = "ERROR: Your file couldn't be uploaded to the epistle Web API because it exceeds the maximum file size of 20MB", Title = "Message upload failed" } };
                        errorView.ShowDialog();
                    }
                }
            };

            dialog.ShowDialog();
        }

        private void OnClickedCopyConvoIdToClipboard(object commandParam)
        {
            Clipboard.SetText(ActiveConvo.Id);
            ClipboardTickVisibility = Visibility.Visible;

            if (scheduledHideGreenTickIcon.HasValue)
                methodQ.Cancel(scheduledHideGreenTickIcon.Value);

            scheduledHideGreenTickIcon = methodQ.Schedule(HideGreenTick, DateTime.UtcNow.AddSeconds(3));
        }
    }
}