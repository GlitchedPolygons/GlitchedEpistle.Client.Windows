using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using GlitchedPolygons.Services.MethodQ;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Messages;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using Prism.Events;
using Microsoft.Win32;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class ActiveConvoViewModel : ViewModel
    {
        #region Constants

        private const long MAX_FILE_SIZE_BYTES = 20971520;
        private const string MSG_TIMESTAMP_FORMAT = "dd.MM.yyyy HH:mm";
        private static readonly char[] MSG_TRIM_CHARS = { '\n', '\r', '\t' };
        private static readonly TimeSpan MSG_PULL_FREQUENCY = TimeSpan.FromMilliseconds(1111);

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

        private Visibility clipboardTickVisibility = Visibility.Hidden;
        public Visibility ClipboardTickVisibility
        {
            get => clipboardTickVisibility;
            set => Set(ref clipboardTickVisibility, value);
        }

        private ObservableCollection<MessageViewModel> messages = new AsyncObservableCollection<MessageViewModel>();
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
            set
            {
                if (scheduledUpdateRoutine.HasValue)
                    methodQ.Cancel(scheduledUpdateRoutine.Value);

                Messages = new AsyncObservableCollection<MessageViewModel>();
                activeConvo = value;
                LoadLocalMessages();
                PullNewestMessages();
                StartAutomaticPulling();
            }
        }

        private bool pulling;
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
            
            eventAggregator.GetEvent<LogoutEvent>().Subscribe(StopAutomaticPulling);

            settings.Load();
        }
        
        ~ActiveConvoViewModel()
        {
            if (scheduledUpdateRoutine.HasValue)
                methodQ?.Cancel(scheduledUpdateRoutine.Value);
        }

        private void StopAutomaticPulling()
        {
            if (scheduledUpdateRoutine.HasValue)
                methodQ.Cancel(scheduledUpdateRoutine.Value);
        }

        private void StartAutomaticPulling()
        {
            StopAutomaticPulling();
        }

        private void LoadLocalMessages()
        {
            if (ActiveConvo is null || string.IsNullOrEmpty(ActiveConvo.Id))
            {
                return;
            }

            string dir = Path.Combine(Paths.CONVOS_DIRECTORY, ActiveConvo.Id);
            if (!Directory.Exists(dir))
            {
                return;
            }

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

                string encryptedMessage = crypto.EncryptMessage
                (
                    messageJson: messageBodyJson.ToString(Formatting.None),
                    recipientPublicRsaKey: RSAParametersExtensions.FromXml(key.Item2)
                );

                messageBodiesJson[key.Item1] = encryptedMessage;
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
                TimestampUTC = DateTime.UtcNow,
                Body = messageBody.ToString()
            };

            AddMessageToView(message);
            WriteMessageToDisk(message);

            return await convoService.PostMessage
            (
                convoId: ActiveConvo.Id,
                messageDto: new PostMessageParamsDto
                {
                    UserId = user.Id,
                    SenderName = username,
                    Auth = user.Token.Item2,
                    TimestampUTC = message.TimestampUTC,
                    ConvoPasswordHash = ActiveConvo.PasswordSHA512,
                    MessageBodiesJson = messageBodiesJson.ToString(Formatting.None)
                }
            );
        }

        private void WriteMessageToDisk(Message message)
        {
            File.WriteAllText
            (
                contents: JsonConvert.SerializeObject(message),
                path: Path.Combine(Paths.CONVOS_DIRECTORY, ActiveConvo.Id, message.TimestampUTC.ToString("yyyyMMddHHmmssff"))
            );
        }

        private void AddMessageToView(Message message)
        {
            if (message is null)
            {
                return;
            }

            var messageViewModel = new MessageViewModel(methodQ)
            {
                SenderId = message.SenderId,
                SenderName = message.SenderName,
                TimestampDateTimeUTC = message.TimestampUTC,
                Timestamp = message.TimestampUTC.ToLocalTime().ToString(MSG_TIMESTAMP_FORMAT),
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

        private async void PullNewestMessages()
        {
            if (pulling || ActiveConvo is null || user is null)
            {
                return;
            }

            pulling = true;

            int i = Messages.Count > 0 
                ? await convoService.IndexOf(ActiveConvo.Id, ActiveConvo.PasswordSHA512, user.Id, user.Token.Item2, Messages.Last().Id) 
                : 0;

            if (i < 0)
            {
                pulling = false;
                return;
            }

            var retrievedMessages = await convoService.GetConvoMessages(ActiveConvo.Id, ActiveConvo.PasswordSHA512, user.Id, user.Token.Item2, i);
            if (retrievedMessages is null || retrievedMessages.Length == 0)
            {
                return;
            }

            // Add the retrieved messages only to the chatroom if it does not contain them yet (mistakes are always possible; safe is safe).
            Parallel.ForEach(retrievedMessages.Where(m1 => Messages.All(m2 => m2.Id != m1.Id)).OrderBy(m => m.TimestampUTC), message =>
            {
                AddMessageToView(message);
                WriteMessageToDisk(message);
            });

            pulling = false;
        }

        private async void OnSendText(object commandParam)
        {
            var textBox = commandParam as TextBox;
            if (textBox is null)
            {
                return;
            }

            string messageText = textBox.Text;
            if (string.IsNullOrEmpty(messageText))
            {
                return;
            }

            messageText = messageText.TrimEnd(MSG_TRIM_CHARS).TrimStart(MSG_TRIM_CHARS);
            JObject messageBodyJson = new JObject
            {
                ["text"] = messageText
            };

            if (await SubmitMessage(messageBodyJson))
            {
                textBox.Clear();
                messageBodyJson["text"] = null;
                messageBodyJson = null;
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

                    if (file.LongLength < MAX_FILE_SIZE_BYTES)
                    {
                        var messageBodyJson = new JObject
                        {
                            ["fileName"] = Path.GetFileName(_dialog.FileName),
                            ["fileBase64"] = Convert.ToBase64String(file)
                        };

                        if (!await SubmitMessage(messageBodyJson))
                        {
                            var errorView = new InfoDialogView { DataContext = new InfoDialogViewModel { OkButtonText = "Okay :/", Text = "ERROR: Your file couldn't be uploaded to the epistle Web API", Title = "Message upload failed" } };
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
