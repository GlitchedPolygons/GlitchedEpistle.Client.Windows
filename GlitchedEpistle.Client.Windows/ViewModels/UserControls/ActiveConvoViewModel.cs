﻿#define PARALLEL_LOAD

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;

using GlitchedPolygons.Services.MethodQ;
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
        private static readonly TimeSpan MSG_PULL_FREQUENCY = TimeSpan.FromMilliseconds(666);

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
        public ICommand PressedEscapeCommand { get; }
        public ICommand CopyConvoIdToClipboardCommand { get; }
        #endregion

        #region UI Bindings
        private bool canSend;
        public bool CanSend
        {
            get => canSend;
            set => Set(ref canSend, value);
        }

        private bool pulling;
        public bool Pulling
        {
            get => pulling;
            set => Set(ref pulling, value);
        }

        private string name;
        public string Name
        {
            get => name;
            set => Set(ref name, value);
        }

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
                // Prevent the user from submitting new messages whilst changing convos.
                CanSend = false;
                StopAutomaticPulling();

                // Reset the view's message collection and 
                // load the local sqlite message repository.
                Messages = new ObservableCollection<MessageViewModel>();
                repo = new MessageRepositorySQLite($"Data Source={Path.Combine(Paths.CONVOS_DIRECTORY, value.Id + ".db")};Version=3;");

                activeConvo = value;
                Name = value?.Name;

                // Convo expiration check 
                // (adapt the title label accordingly).
                DateTime? exp = value?.ExpirationUTC;
                if (exp.HasValue)
                {
                    if (DateTime.UtcNow > exp.Value)
                    {
                        Name += " (EXPIRED)";
                    }
                    else if ((exp.Value - DateTime.UtcNow).TotalDays < 3)
                    {
                        Name += " (EXPIRES SOON)";
                    }
                }

                // Decrypt the messages that are already stored 
                // locally on the device and load them into the view.
                // Then, resume the user's ability to send messages once done.
                LoadLocalMessages();
                CanSend = true;
            }
        }

        private ulong? scheduledUpdateRoutine;
        private ulong? scheduledHideGreenTickIcon;

        private MessageRepositorySQLite repo;

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
            PressedEscapeCommand = new DelegateCommand(OnPressedEscape);
            CopyConvoIdToClipboardCommand = new DelegateCommand(OnClickedCopyConvoIdToClipboard);

            eventAggregator.GetEvent<LogoutEvent>().Subscribe(StopAutomaticPulling);
            eventAggregator.GetEvent<ChangedConvoMetadataEvent>().Subscribe(OnChangedConvoMetadata);

            settings.Load();

            if (!Directory.Exists(Paths.CONVOS_DIRECTORY))
            {
                Directory.CreateDirectory(Paths.CONVOS_DIRECTORY);
            }
        }

        ~ActiveConvoViewModel()
        {
            StopAutomaticPulling();
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
            scheduledUpdateRoutine = methodQ.Schedule(async () => { await PullNewestMessages(); }, MSG_PULL_FREQUENCY);
        }

        private MessageViewModel DecryptMessage(Message message)
        {
            if (message is null)
            {
                return null;
            }

            JToken json = JToken.Parse(crypto.DecryptMessage(message.Body, user.PrivateKey));
            if (json == null)
            {
                return null;
            }

            var messageViewModel = new MessageViewModel(methodQ)
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderName = message.SenderName,
                TimestampDateTimeUTC = message.TimestampUTC,
                Timestamp = message.TimestampUTC.ToLocalTime().ToString(MSG_TIMESTAMP_FORMAT),
                Text = json["text"]?.Value<string>(),
                FileName = json["fileName"]?.Value<string>()
            };

            string fileBase64 = json["fileBase64"]?.Value<string>();
            messageViewModel.FileBytes = fileBase64.NullOrEmpty() ? null : Convert.FromBase64String(fileBase64);

            return messageViewModel;
        }

        private void LoadLocalMessages()
        {
            if (ActiveConvo is null || ActiveConvo.Id.NullOrEmpty())
            {
                return;
            }

            DecryptingVisibility = Visibility.Visible;

            Task.Run(async () =>
            {
                var decryptedMessages = new ConcurrentBag<MessageViewModel>();

#if PARALLEL_LOAD
                Parallel.ForEach(await repo.GetAll(), message =>
                {
                    try
                    {
                        decryptedMessages.Add(DecryptMessage(message));
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"{nameof(ActiveConvoViewModel)}::{nameof(LoadLocalMessages)}: Failed to load message into convo chatroom view. Error message: {e}");
                    }
                });
#else
                foreach (var message in await repo.GetAll())
                {
                    try
                    {
                        decryptedMessages.Add(DecryptMessage(message));
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"{nameof(ActiveConvoViewModel)}::{nameof(LoadLocalMessages)}: Failed to load message into convo chatroom view. Error message: {e}");
                    }
                }
#endif

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    DecryptingVisibility = Visibility.Hidden;
                    Messages.AddRange(decryptedMessages.Where(m1 => Messages.All(m2 => m2.Id != m1.Id)).OrderBy(m => m.TimestampDateTimeUTC).ToArray());
                    StartAutomaticPulling();
                });
            });
        }

        /// <summary>
        /// Updates the convo's metadata. Returns <c>true</c> if the metadata was updated (thus something changed), or <c>false</c> if nothing changed.
        /// </summary>
        /// <returns>Whether the convo metadata was updated or not.</returns>
        private async Task<bool> UpdateConvoMetadata()
        {
            var convo = convoProvider[ActiveConvo.Id];
            var metadata = await convoService.GetConvoMetadata(ActiveConvo.Id, ActiveConvo.PasswordSHA512, user.Id, user.Token.Item2);

            if (convo is null || metadata is null || convo.Equals(metadata))
            {
                return false;
            }

            convo.Name = ActiveConvo.Name = metadata.Name;
            convo.CreatorId = ActiveConvo.CreatorId = metadata.CreatorId;
            convo.Description = ActiveConvo.Description = metadata.Description;
            convo.ExpirationUTC = ActiveConvo.ExpirationUTC = metadata.ExpirationUTC;
            convo.CreationTimestampUTC = ActiveConvo.CreationTimestampUTC = metadata.CreationTimestampUTC;
            convo.BannedUsers = ActiveConvo.BannedUsers = metadata.BannedUsers.Split(',').ToList();
            convo.Participants = ActiveConvo.Participants = metadata.Participants.Split(',').ToList();

            convoProvider.Save();

            eventAggregator.GetEvent<ChangedConvoMetadataEvent>().Publish(convo.Id);

            return true;
        }

        /// <summary>
        /// Submits the message body to the convo. 
        /// Returns whether the message was submitted successfully or not.
        /// </summary>
        /// <param name="messageBodyJson">The message body's json.</param>
        /// <returns>Whether the message was submitted successfully or not.</returns>
        private async Task<bool> SubmitMessage(JObject messageBodyJson)
        {
            if (ActiveConvo is null)
            {
                logger.LogError($"Tried to submit a message from an {nameof(ActiveConvoViewModel)} whose {nameof(ActiveConvo)} is null! Something's wrong... please analyze this behaviour!");
                return false;
            }

            EncryptingVisibility = Visibility.Visible;

            await UpdateConvoMetadata();

            var messageBodiesJson = new JObject();
            var messageBodyJsonString = messageBodyJson.ToString(Formatting.None);
            var encryptedMessagesBag = new ConcurrentBag<Tuple<string, string>>();

            // Get the keys of all convo participants here.
            List<Tuple<string, string>> keys = await userService.GetUserPublicKeyXml(user.Id, ActiveConvo.GetParticipantIdsCommaSeparated(), user.Token.Item2);

            // Encrypt the message for every convo participant individually
            // and put the result in a temporary concurrent bag.
            Parallel.ForEach(keys, key =>
            {
                if (key != null && key.Item1.NotNullNotEmpty() && key.Item2.NotNullNotEmpty() && messageBodyJsonString.NotNullNotEmpty())
                {
                    string encryptedMessage = crypto.EncryptMessage(
                        messageJson: messageBodyJsonString,
                        recipientPublicRsaKey: RSAParametersExtensions.FromXmlString(key.Item2)
                    );

                    encryptedMessagesBag.Add(new Tuple<string, string>(key.Item1, encryptedMessage));
                }
            });

            foreach (Tuple<string, string> encryptedMessage in encryptedMessagesBag)
            {
                messageBodiesJson[encryptedMessage.Item1] = encryptedMessage.Item2;
            }

            EncryptingVisibility = Visibility.Hidden;

            JToken ownMessageBody = messageBodiesJson[user.Id];
            if (ownMessageBody is null)
            {
                return false;
            }

            string username = settings["Username"];

            var postParamsDto = new PostMessageParamsDto
            {
                UserId = user.Id,
                SenderName = username,
                Auth = user.Token.Item2,
                ConvoPasswordSHA512 = ActiveConvo.PasswordSHA512,
                MessageBodiesJson = messageBodiesJson.ToString(Formatting.None)
            };

            bool success = await convoService.PostMessage(ActiveConvo.Id, postParamsDto);

            return success;
        }

        /// <summary>
        /// Pulls the convo's newest messages from the server.
        /// Returns whether any new messages were pulled successfully or not.
        /// </summary>
        /// <returns>Whether any new messages were pulled successfully or not.</returns>
        private async Task<bool> PullNewestMessages()
        {
            if (Pulling || ActiveConvo is null || user is null)
            {
                return false;
            }

            Pulling = true;

            // Pull convo metadata first.
            await UpdateConvoMetadata();

            // Pull newest messages.
            Message[] retrievedMessages = await convoService.GetConvoMessages(
                convoId: ActiveConvo.Id,
                convoPasswordSHA512: ActiveConvo.PasswordSHA512,
                userId: user?.Id,
                auth: user?.Token?.Item2,
                tailId: Messages.Count == 0 ? null : Messages.Last()?.Id
            );

            if (retrievedMessages is null || retrievedMessages.Length == 0)
            {
                Pulling = false;
                return false;
            }

            DecryptingVisibility = Visibility.Visible;

            var decryptedMessages = new ConcurrentBag<MessageViewModel>();

            Parallel.ForEach(retrievedMessages, message =>
            {
                // Add the retrieved messages to the chatroom
                // only if it does not contain them yet
                // (mistakes are always possible; safe is safe).
                if (Messages.All(m => m.Id != message.Id))
                {
                    decryptedMessages.Add(DecryptMessage(message));
                }
            });

            // Newly pulled messages should be
            // written out to a file on disk.
            bool success = await repo.AddRange(retrievedMessages);

            DecryptingVisibility = Visibility.Hidden;

            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.AddRange(decryptedMessages.Where(m1 => Messages.All(m2 => m2.Id != m1.Id)).OrderBy(m => m.TimestampDateTimeUTC).ToArray());
                Pulling = false;
            });

            return success;
        }

        private void OnSendText(object commandParam)
        {
            var textBox = commandParam as TextBox;
            if (textBox is null)
            {
                return;
            }

            string messageText = textBox.Text;
            if (messageText.NullOrEmpty())
            {
                return;
            }

            textBox.Clear();
            textBox.Focus();
            textBox.SelectAll();

            Task.Run(async () =>
            {
                var messageBodyJson = new JObject { ["text"] = messageText.TrimEnd(MSG_TRIM_CHARS).TrimStart(MSG_TRIM_CHARS) };

                bool success = await SubmitMessage(messageBodyJson);

                if (!success)
                {
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        var errorView = new InfoDialogView { DataContext = new InfoDialogViewModel { OkButtonText = "Okay :/", Text = "ERROR: Your text message couldn't be uploaded to the epistle Web API", Title = "Message upload failed" } };
                        errorView.ShowDialog();
                    });
                }

                messageBodyJson["text"] = messageBodyJson = null;
            });
        }

        private void OnSendFile(object commandParam)
        {
            var dialog = new OpenFileDialog { Multiselect = false, Title = "Epistle - Select the file you want to send", InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) };
            dialog.FileOk += OnSelectedFile;
            dialog.ShowDialog();
        }

        private void OnSelectedFile(object sender, EventArgs e)
        {
            var dialog = sender as OpenFileDialog;
            if (dialog is null || dialog.FileName.NullOrEmpty())
            {
                return;
            }

            OnDragAndDropFile(dialog.FileName);
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

        private void OnPressedEscape(object commandParam)
        {
            var messagesListBox = commandParam as ListBox;
            if (messagesListBox is null)
            {
                return;
            }

            if (VisualTreeHelper.GetChildrenCount(messagesListBox) <= 0)
            {
                return;
            }

            var border = (Border)VisualTreeHelper.GetChild(messagesListBox, 0);
            var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
            scrollViewer.ScrollToBottom();
        }

        private void OnChangedConvoMetadata(string convoId)
        {
            var convo = convoProvider[convoId];
            if (convo != null)
            {
                Name = convo.Name;
            }
        }

        public void OnDragAndDropFile(string filePath)
        {
            if (filePath.NullOrEmpty())
            {
                return;
            }

            Task.Run(async () =>
            {
                byte[] file = File.ReadAllBytes(filePath);

                if (file.LongLength < MAX_FILE_SIZE_BYTES)
                {
                    var messageBodyJson = new JObject
                    {
                        ["fileName"] = Path.GetFileName(filePath),
                        ["fileBase64"] = Convert.ToBase64String(file)
                    };

                    bool success = await SubmitMessage(messageBodyJson);

                    if (!success)
                    {
                        Application.Current?.Dispatcher?.Invoke(() =>
                        {
                            var errorView = new InfoDialogView { DataContext = new InfoDialogViewModel { OkButtonText = "Okay :/", Text = "ERROR: Your file couldn't be uploaded to the epistle Web API", Title = "Message upload failed" } };
                            errorView.ShowDialog();
                        });
                    }

                    messageBodyJson["fileBase64"] = messageBodyJson["fileName"] = messageBodyJson = null;
                }
                else
                {
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        var errorView = new InfoDialogView { DataContext = new InfoDialogViewModel { OkButtonText = "Okay :/", Text = "ERROR: Your file couldn't be uploaded to the epistle Web API because it exceeds the maximum file size of 20MB", Title = "Message upload failed" } };
                        errorView.ShowDialog();
                    });
                }
            });
        }

        private void HideGreenTick()
        {
            ClipboardTickVisibility = Visibility.Hidden;
            scheduledHideGreenTickIcon = null;
        }
    }
}
