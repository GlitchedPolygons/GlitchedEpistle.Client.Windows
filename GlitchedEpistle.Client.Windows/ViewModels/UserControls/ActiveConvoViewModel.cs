﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Messages;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.Services.MethodQ;

using Microsoft.Win32;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Prism.Events;

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
        private bool canSend;
        public bool CanSend { get => canSend; set => Set(ref canSend, value); }

        private Visibility clipboardTickVisibility = Visibility.Hidden;
        public Visibility ClipboardTickVisibility
        {
            get => clipboardTickVisibility;
            set => Set(ref clipboardTickVisibility, value);
        }

        private Visibility encryptingVisibility = Visibility.Hidden;
        public Visibility EncryptingVisibility
        {
            get => encryptingVisibility;
            set => Set(ref encryptingVisibility, value);
        }

        private Visibility decryptingVisibility = Visibility.Hidden;
        public Visibility DecryptingVisibility
        {
            get => decryptingVisibility;
            set => Set(ref decryptingVisibility, value);
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
            set
            {
                CanSend = false;
                StopAutomaticPulling();
                Messages = new ObservableCollection<MessageViewModel>();
                activeConvo = value;
                LoadLocalMessages();
                PullNewestMessages();
                StartAutomaticPulling();
                CanSend = true;
            }
        }

        private bool pulling;
        private ulong? scheduledUpdateRoutine;
        private ulong? scheduledHideGreenTickIcon;
        private ConcurrentBag<MessageViewModel> msgQueue = new ConcurrentBag<MessageViewModel>();

        public ActiveConvoViewModel(User user, IConvoService convoService, IConvoProvider convoProvider, IEventAggregator eventAggregator, IMethodQ methodQ, IUserService userService, IMessageCryptography crypto, ISettings settings, ILogger logger)
        {
            #region Injections
            this.user = user;
            this.logger = logger;
            this.crypto = crypto;
            this.methodQ = methodQ;
            this.settings = settings;
            this.userService = userService;
            this.convoService = convoService;
            this.convoProvider = convoProvider;
            this.eventAggregator = eventAggregator;
            #endregion

            SendTextCommand = new DelegateCommand(OnSendText);
            SendFileCommand = new DelegateCommand(OnSendFile);
            CopyConvoIdToClipboardCommand = new DelegateCommand(OnClickedCopyConvoIdToClipboard);
            methodQ.Schedule(() => Application.Current?.Dispatcher?.Invoke(TransferQueuedMessagesToUI), MSG_PULL_FREQUENCY);
            eventAggregator.GetEvent<LogoutEvent>().Subscribe(StopAutomaticPulling);
            settings.Load();
        }

        ~ActiveConvoViewModel()
        {
            if (scheduledUpdateRoutine.HasValue)
            {
                methodQ?.Cancel(scheduledUpdateRoutine.Value);
            }
        }

        private void StopAutomaticPulling()
        {
            if (scheduledUpdateRoutine.HasValue)
            {
                methodQ.Cancel(scheduledUpdateRoutine.Value);
            }
        }

        private void StartAutomaticPulling()
        {
            StopAutomaticPulling();
            scheduledUpdateRoutine = methodQ.Schedule(PullNewestMessages, MSG_PULL_FREQUENCY);
        }

        private void TransferQueuedMessagesToUI()
        {
            if (msgQueue.Count <= 0)
            {
                return;
            }

            Messages.AddRange(msgQueue.Where(m1 => Messages.All(m2 => m2.Id != m1.Id)).OrderBy(m => m.TimestampDateTimeUTC));
            msgQueue = new ConcurrentBag<MessageViewModel>();
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

            DecryptingVisibility = Visibility.Visible;

            Parallel.ForEach(Directory.GetFiles(dir), file =>
            {
                try
                {
                    Message message = JsonConvert.DeserializeObject<Message>(File.ReadAllText(file));
                    if (message != null)
                    {
                        AddMessageToView(message);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError($"{nameof(ActiveConvoViewModel)}::{nameof(LoadLocalMessages)}: Failed to load message into convo chatroom view. Error message: {e}");
                }
            });

            DecryptingVisibility = Visibility.Hidden;
        }

        private Task EncryptMessageBodyForUser(ConcurrentBag<Tuple<string, string>> resultsBag, Tuple<string, string> userKeyPair, string messageBodyJson)
        {
            string encryptedMessage = crypto.EncryptMessage(
                messageBodyJson,
                RSAParametersExtensions.FromXmlString(userKeyPair.Item2)
            );
            resultsBag.Add(new Tuple<string, string>(userKeyPair.Item1, encryptedMessage));
            return Task.CompletedTask;
        }

        private async Task<bool> SubmitMessage(JObject messageBodyJson)
        {
            if (ActiveConvo is null)
            {
                logger.LogError($"Tried to submit a message from an {nameof(ActiveConvoViewModel)} whose {nameof(ActiveConvo)} is null! Something's wrong... please analyze this behaviour!");
                return false;
            }

            CanSend = false;
            EncryptingVisibility = Visibility.Visible;

            JObject messageBodiesJson = new JObject();
            ConcurrentBag<Tuple<string, string>> bag = new ConcurrentBag<Tuple<string, string>>();
            List<Task> tasks = new List<Task>(ActiveConvo.Participants.Count);
            string messageBodyJsonString = messageBodyJson.ToString(Formatting.None);
            List<Tuple<string, string>> keys = await userService.GetUserPublicKeyXml(user.Id, ActiveConvo.GetParticipantIdsCommaSeparated(), user.Token.Item2);

            foreach (Tuple<string, string> key in keys)
            {
                if (key is null || key.Item1.NullOrEmpty() || key.Item2.NullOrEmpty())
                {
                    continue;
                }

                tasks.Add(EncryptMessageBodyForUser(bag, key, messageBodyJsonString));
            }

            await Task.WhenAll(tasks);

            foreach (Tuple<string, string> encryptedMessage in bag)
            {
                messageBodiesJson[encryptedMessage.Item1] = encryptedMessage.Item2;
            }

            EncryptingVisibility = Visibility.Hidden;

            JToken ownMessageBody = messageBodiesJson[user.Id];
            if (ownMessageBody is null)
            {
                CanSend = true;
                return false;
            }

            string username = settings["Username"];

            Message message = new Message
            {
                SenderId = user.Id, 
                SenderName = username, 
                TimestampUTC = DateTime.UtcNow, 
                Body = ownMessageBody.ToString()
            };

            StopAutomaticPulling();

            await AddMessageToView(message);
            await WriteMessageToDisk(message);

            bool success = await convoService.PostMessage
            (
                convoId: ActiveConvo.Id,
                messageDto: new PostMessageParamsDto
                {
                    UserId = user.Id,
                    SenderName = username,
                    Auth = user.Token.Item2,
                    TimestampUTC = message.TimestampUTC,
                    ConvoPasswordSHA512 = ActiveConvo.PasswordSHA512,
                    MessageBodiesJson = messageBodiesJson.ToString(Formatting.None)
                }
            );

            CanSend = true;
            StartAutomaticPulling();
            return success;
        }

        private Task WriteMessageToDisk(Message message)
        {
            File.WriteAllText(
                contents: JsonConvert.SerializeObject(message),
                path: Path.Combine(Paths.CONVOS_DIRECTORY, ActiveConvo.Id, message.TimestampUTC.ToString("yyyyMMddHHmmssff"))
            );
            return Task.CompletedTask;
        }

        private Task AddMessageToView(Message message)
        {
            if (message is null)
            {
                return Task.CompletedTask;
            }

            JToken json = JToken.Parse(crypto.DecryptMessage(message.Body, user.PrivateKey));
            if (json is null)
            {
                return Task.CompletedTask;
            }

            MessageViewModel messageViewModel = new MessageViewModel(methodQ)
            {
                SenderId = message.SenderId,
                SenderName = message.SenderName,
                TimestampDateTimeUTC = message.TimestampUTC,
                Timestamp = message.TimestampUTC.ToLocalTime().ToString(MSG_TIMESTAMP_FORMAT),
                Text = json["text"]?.Value<string>(),
                FileName = json["fileName"]?.Value<string>()
            };

            string fileBase64 = json["fileBase64"]?.Value<string>();
            messageViewModel.FileBytes = fileBase64.NullOrEmpty() ? null : Convert.FromBase64String(fileBase64);
            msgQueue.Add(messageViewModel);

            return Task.CompletedTask;
        }

        private void PullNewestMessages()
        {
            if (pulling || ActiveConvo is null || user is null)
            {
                return;
            }

            pulling = true;

            Task.Run(async () =>
            {
                int i = 0;
                if (Messages.Count > 0)
                {
                    i = await convoService.IndexOf(ActiveConvo.Id, ActiveConvo.PasswordSHA512, user.Id, user.Token.Item2, Messages.Last().Id);
                }

                if (i < 0)
                {
                    pulling = false;
                    return;
                }

                Message[] retrievedMessages = await convoService.GetConvoMessages(ActiveConvo.Id, ActiveConvo.PasswordSHA512, user?.Id, user?.Token?.Item2, i);
                if (retrievedMessages is null || retrievedMessages.Length == 0)
                {
                    pulling = false;
                    return;
                }

                DecryptingVisibility = Visibility.Visible;

                List<Task> tasks = new List<Task>(retrievedMessages.Length * 2);

                foreach (Message message in retrievedMessages)
                {
                    // Add the retrieved messages only to the chatroom
                    // if it does not contain them yet (mistakes are always possible; safe is safe).
                    if (Messages.Any(m => m.Id == message.Id))
                    {
                        continue;
                    }

                    tasks.Add(AddMessageToView(message));
                    tasks.Add(WriteMessageToDisk(message));
                }

                await Task.WhenAll(tasks);
                pulling = false;
                DecryptingVisibility = Visibility.Hidden;
            });
        }

        private async void OnSendText(object commandParam)
        {
            TextBox textBox = commandParam as TextBox;
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
            JObject messageBodyJson = new JObject { ["text"] = messageText };

            if (await SubmitMessage(messageBodyJson))
            {
                textBox.Clear();
                messageBodyJson["text"] = null;
                messageBodyJson = null;
            }
            else
            {
                InfoDialogView errorView = new InfoDialogView { DataContext = new InfoDialogViewModel { OkButtonText = "Okay :/", Text = "ERROR: Your message couldn't be uploaded to the epistle Web API", Title = "Message upload failed" } };
                errorView.ShowDialog();
            }
        }

        private void OnSendFile(object commandParam)
        {
            OpenFileDialog dialog = new OpenFileDialog { Multiselect = false, Title = "Epistle - Select the file you want to send", InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) };
            dialog.FileOk += OnSelectedFile;
            dialog.ShowDialog();
        }

        private void OnSelectedFile(object sender, EventArgs e)
        {
            if (sender is OpenFileDialog _dialog && !string.IsNullOrEmpty(_dialog.FileName))
            {
                Task.Run(async () =>
                {
                    byte[] file = File.ReadAllBytes(_dialog.FileName);

                    if (file.LongLength < MAX_FILE_SIZE_BYTES)
                    {
                        JObject messageBodyJson = new JObject { ["fileName"] = Path.GetFileName(_dialog.FileName), ["fileBase64"] = Convert.ToBase64String(file) };

                        if (!await SubmitMessage(messageBodyJson))
                        {
                            InfoDialogView errorView = new InfoDialogView { DataContext = new InfoDialogViewModel { OkButtonText = "Okay :/", Text = "ERROR: Your file couldn't be uploaded to the epistle Web API", Title = "Message upload failed" } };
                            errorView.ShowDialog();
                        }
                    }
                    else
                    {
                        InfoDialogView errorView = new InfoDialogView { DataContext = new InfoDialogViewModel { OkButtonText = "Okay :/", Text = "ERROR: Your file couldn't be uploaded to the epistle Web API because it exceeds the maximum file size of 20MB", Title = "Message upload failed" } };
                        errorView.ShowDialog();
                    }
                });
            }
        }

        private void OnClickedCopyConvoIdToClipboard(object commandParam)
        {
            Clipboard.SetText(ActiveConvo.Id);
            ClipboardTickVisibility = Visibility.Visible;

            if (scheduledHideGreenTickIcon.HasValue)
            {
                methodQ.Cancel(scheduledHideGreenTickIcon.Value);
            }

            scheduledHideGreenTickIcon = methodQ.Schedule(HideGreenTick, DateTime.UtcNow.AddSeconds(3));
        }

        private void HideGreenTick()
        {
            ClipboardTickVisibility = Visibility.Hidden;
            scheduledHideGreenTickIcon = null;
        }
    }
}
