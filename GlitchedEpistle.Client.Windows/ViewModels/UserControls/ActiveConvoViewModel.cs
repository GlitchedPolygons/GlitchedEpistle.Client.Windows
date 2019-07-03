#define PARALLEL_LOAD
// Comment out the above line to load/decrypt messages synchronously.

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
using System.Windows.Threading;

using GlitchedPolygons.Services.MethodQ;
using GlitchedPolygons.RepositoryPattern;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Models.DTOs;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Users;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Cryptography.Messages;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;

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
        private const int MSG_COLLECTION_SIZE = 20;
        private const string MSG_TIMESTAMP_FORMAT = "dd.MM.yyyy HH:mm";
        private static readonly char[] MSG_TRIM_CHARS = { '\n', '\r', '\t' };
        private static readonly TimeSpan MSG_PULL_FREQUENCY = TimeSpan.FromMilliseconds(420);
        private static readonly TimeSpan METADATA_PULL_FREQUENCY = TimeSpan.FromMilliseconds(30000);

        // Injections:
        private readonly User user;
        private readonly ILogger logger;
        private readonly IMethodQ methodQ;
        private readonly ISettings settings;
        private readonly IUserService userService;
        private readonly IConvoService convoService;
        private readonly IMessageCryptography crypto;
        private readonly IEventAggregator eventAggregator;
        private readonly IRepository<Convo, string> convoProvider;
        private readonly IConvoPasswordProvider convoPasswordProvider;
        #endregion

        #region Commands
        public ICommand SendTextCommand { get; }
        public ICommand SendFileCommand { get; }
        public ICommand PressedEscapeCommand { get; }
        public ICommand ScrollToBottomCommand { get; }
        public ICommand LoadPreviousMessagesCommand { get; }
        public ICommand CopyConvoIdToClipboardCommand { get; }
        #endregion

        #region UI Bindings
        private bool canSend;
        public bool CanSend
        {
            get => canSend;
            set => Set(ref canSend, value);
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                // Convo expiration check 
                // (adapt the title label accordingly).
                DateTime? exp = ActiveConvo?.ExpirationUTC;
                if (exp.HasValue)
                {
                    if (DateTime.UtcNow > exp.Value)
                    {
                        value += " (EXPIRED)";
                    }
                    else if ((exp.Value - DateTime.UtcNow).TotalDays < 3)
                    {
                        value += " (EXPIRES SOON)";
                    }
                }

                Set(ref name, value);
            }
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

        private volatile bool pulling;
        private volatile int pageIndex;
        private DateTime lastMetadataPull;
        private ulong? scheduledHideGreenTickIcon;
        private IMessageRepository messageRepository;

        private Convo activeConvo;
        public Convo ActiveConvo
        {
            get => activeConvo;
            set
            {
                // Prevent the user from both pulling and
                // submitting new messages whilst changing convos.
                CanSend = false;
                StopAutomaticPulling();

                Messages = new ObservableCollection<MessageViewModel>();
                messageRepository = new MessageRepositorySQLite($"Data Source={Path.Combine(Paths.CONVOS_DIRECTORY, value.Id + ".db")};Version=3;");

                activeConvo = value;
                Name = value.Name;

                // Decrypt the messages that are already stored 
                // locally on the device and load them into the view.
                // Then, resume the user's ability to send messages once done.
                Task.Run(async () =>
                {
                    var encryptedMessages = await messageRepository.GetLastMessages(MSG_COLLECTION_SIZE);
                    var decryptedMessages = DecryptMessages(encryptedMessages).OrderBy(m => m.TimestampDateTimeUTC);

                    ExecUI(() =>
                    {
                        Messages.AddRange(decryptedMessages);
                        StartAutomaticPulling();
                        CanSend = true;
                    });
                });
            }
        }

        public ActiveConvoViewModel(
            User user,
            IConvoService convoService,
            IEventAggregator eventAggregator,
            IMethodQ methodQ,
            IUserService userService,
            IMessageCryptography crypto,
            ISettings settings,
            ILogger logger,
            IRepository<Convo, string> convoProvider,
            IConvoPasswordProvider convoPasswordProvider)
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
            this.convoPasswordProvider = convoPasswordProvider;
            this.eventAggregator = eventAggregator;
            #endregion

            SendTextCommand = new DelegateCommand(OnSendText);
            SendFileCommand = new DelegateCommand(OnSendFile);
            PressedEscapeCommand = new DelegateCommand(OnPressedEscape);
            ScrollToBottomCommand = new DelegateCommand(OnScrollToBottom);
            LoadPreviousMessagesCommand = new DelegateCommand(OnClickedLoadPreviousMessages);
            CopyConvoIdToClipboardCommand = new DelegateCommand(OnClickedCopyConvoIdToClipboard);

            eventAggregator.GetEvent<LogoutEvent>().Subscribe(StopAutomaticPulling);
            eventAggregator.GetEvent<ChangedConvoMetadataEvent>().Subscribe(OnChangedConvoMetadata);

            settings.Load();
        }

        ~ActiveConvoViewModel()
        {
            StopAutomaticPulling();
        }

        /// <summary>
        /// Shorthand for <c>Application.Current?.Dispatcher?.Invoke(Action, DispatcherPriority);</c>
        /// </summary>
        /// <param name="action">What you want to execute on the UI thread.</param>
        /// <param name="priority">The <see cref="DispatcherPriority"/> with which to execute the <see cref="Action"/> on the UI thread.</param>
        private static void ExecUI(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            Application.Current?.Dispatcher?.Invoke(action, priority);
        }

        /// <summary>
        /// Stops the <see cref="ActiveConvoViewModel"/> from automatically pulling messages.
        /// </summary>
        private void StopAutomaticPulling()
        {
            pulling = false;
        }

        /// <summary>
        /// Starts the automatic message pull cycle,
        /// which also truncates the view's message collection size when needed.
        /// </summary>
        private void StartAutomaticPulling()
        {
            StopAutomaticPulling();

            pulling = true;

            // Start message pull cycle:
            Task.Run(async () =>
            {
                while (pulling)
                {
                    await Task.Delay(MSG_PULL_FREQUENCY);

                    var pulledMessages = await PullNewestMessages();
                    if (pulledMessages.Length == 0)
                    {
                        continue;
                    }

                    // Add the pulled messages to the local sqlite db.
                    if (!await messageRepository.AddRange(pulledMessages))
                    {
                        logger.LogError($"{nameof(ActiveConvoViewModel)}::<<AutomaticPullCycle>>: The retrieved messages (from message id {pulledMessages[0]?.Id} onwards) could not be added to the local sqlite db on disk. Reason unknown...");
                        continue;
                    }

                    // Decrypt and add the retrieved messages to the chatroom UI.
                    var decryptedMessages = DecryptMessages(pulledMessages).OrderBy(m => m?.TimestampDateTimeUTC);

                    ExecUI(() =>
                    {
                        Messages.AddRange(decryptedMessages);

                        if (pageIndex == 0)
                        {
                            TruncateMessagesCollection();
                        }
                    }, DispatcherPriority.Send);
                }
            });
        }

        /// <summary>
        /// If there are too many messages loaded into the view,
        /// truncate the messages collection to the latest ones.<para> </para>
        /// Only call this method from the UI thread!
        /// </summary>
        private void TruncateMessagesCollection()
        {
            int messageCount = Messages.Count;
            if (messageCount > MSG_COLLECTION_SIZE * 2)
            {
                Messages = new ObservableCollection<MessageViewModel>(Messages.SkipWhile((msg, i) => i < messageCount - MSG_COLLECTION_SIZE).ToArray());
            }
        }

        /// <summary>
        /// Decrypts a single <see cref="Message"/> into a <see cref="MessageViewModel"/>.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> to decrypt.</param>
        /// <returns>The decrypted <see cref="MessageViewModel"/>, ready to be added to the view.</returns>
        private MessageViewModel DecryptMessage(Message message)
        {
            if (message is null)
            {
                return null;
            }

            string decryptedJson = crypto.DecryptMessage(message.Body, user.PrivateKey);
            JToken json = JToken.Parse(decryptedJson);

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
                FileName = json["fileName"]?.Value<string>(),
                IsOwn = message.SenderId.Equals(user.Id),
            };

            string fileBase64 = json["fileBase64"]?.Value<string>();
            messageViewModel.FileBytes = string.IsNullOrEmpty(fileBase64) ? null : Convert.FromBase64String(fileBase64);

            return messageViewModel;
        }

        /// <summary>
        /// Decrypts multiple <see cref="Message"/>s. Does not guarantee correct order!
        /// </summary>
        /// <param name="encryptedMessages">The <see cref="Message"/>s to decrypt.</param>
        /// <returns>The decrypted <see cref="MessageViewModel"/>s, ready to be added to the view.</returns>
        private IEnumerable<MessageViewModel> DecryptMessages(IEnumerable<Message> encryptedMessages)
        {
            var decryptedMessages = new ConcurrentBag<MessageViewModel>();

#if PARALLEL_LOAD
            Parallel.ForEach(encryptedMessages, message =>
            {
                try
                {
                    var decryptedMessage = DecryptMessage(message);
                    decryptedMessages.Add(decryptedMessage);
                }
                catch (Exception e)
                {
                    logger.LogError($"{nameof(ActiveConvoViewModel)}::{nameof(DecryptMessages)}: Failed to decrypt message {message?.Id}. Error message: {e}");
                }
            });
#else
            foreach (var message in encryptedMessages)
            {
                try
                {
                    decryptedMessages.Add(DecryptMessage(message));
                }
                catch (Exception e)
                {
                    logger.LogError($"{nameof(ActiveConvoViewModel)}::{nameof(DecryptMessages)}: Failed to decrypt message {message?.Id}. Error message: {e}");
                }
            }
#endif
            return decryptedMessages;
        }

        /// <summary>
        /// Pulls the convo's metadata and updates the local copy if something changed.<para> </para>
        /// </summary>
        /// <returns>Returns <c>true</c> if the metadata was updated (thus something changed), or <c>false</c> if nothing changed.</returns>
        private async Task<bool> PullConvoMetadata()
        {
            var convo = await convoProvider.Get(ActiveConvo.Id);
            var metadataDto = await convoService.GetConvoMetadata(ActiveConvo.Id, convoPasswordProvider.GetPasswordSHA512(ActiveConvo.Id), user.Id, user.Token.Item2);

            if (convo is null || metadataDto is null || convo.Equals(metadataDto))
            {
                return false;
            }

            convo.Name = ActiveConvo.Name = metadataDto.Name;
            convo.CreatorId = ActiveConvo.CreatorId = metadataDto.CreatorId;
            convo.Description = ActiveConvo.Description = metadataDto.Description;
            convo.ExpirationUTC = ActiveConvo.ExpirationUTC = metadataDto.ExpirationUTC;
            convo.CreationTimestampUTC = ActiveConvo.CreationTimestampUTC = metadataDto.CreationTimestampUTC;
            convo.BannedUsers = ActiveConvo.BannedUsers = metadataDto.BannedUsers.Split(',').ToList();
            convo.Participants = ActiveConvo.Participants = metadataDto.Participants.Split(',').ToList();

            eventAggregator.GetEvent<ChangedConvoMetadataEvent>().Publish(convo.Id);

            return await convoProvider.Update(convo);
        }

        /// <summary>
        /// Pulls the convo's newest messages from the server.<para> </para>
        /// Returns the pulled <see cref="Message"/>s (or an empty array if no new messages were found).
        /// </summary>
        /// <returns>The pulled <see cref="Message"/>s (or an empty array if no new messages were found).</returns>
        private async Task<Message[]> PullNewestMessages()
        {
            if (ActiveConvo is null || user is null)
            {
                return Array.Empty<Message>();
            }

            try
            {
                // Pull convo metadata first.
                if (DateTime.UtcNow - lastMetadataPull > METADATA_PULL_FREQUENCY)
                {
                    await PullConvoMetadata();
                    lastMetadataPull = DateTime.UtcNow;
                }

                // Pull newest messages.
                Message[] retrievedMessages = await convoService.GetConvoMessages(
                    convoId: ActiveConvo.Id,
                    convoPasswordSHA512: convoPasswordProvider.GetPasswordSHA512(ActiveConvo.Id),
                    userId: user?.Id,
                    auth: user?.Token?.Item2,
                    tailId: await messageRepository.GetLastMessageId()
                );

                if (retrievedMessages is null || retrievedMessages.Length == 0)
                {
                    return Array.Empty<Message>();
                }

                return retrievedMessages.OrderBy(m => m.TimestampUTC).ToArray();
            }
            catch (Exception e)
            {
                logger.LogError($"{nameof(ActiveConvoViewModel)}::{nameof(PullNewestMessages)}: Pull failed. Thrown exception: " + e.ToString());
                return Array.Empty<Message>();
            }
        }

        /// <summary>
        /// Encrypts and submits a <see cref="Message"/> body to the server.<para> </para>
        /// Returns whether the <see cref="Message"/> was submitted successfully or not.
        /// </summary>
        /// <param name="messageBodyJson">The <see cref="Message"/> body's json.</param>
        /// <returns>Whether the <see cref="Message"/> was submitted successfully or not.</returns>
        private async Task<bool> SubmitMessage(JObject messageBodyJson)
        {
            if (ActiveConvo is null)
            {
                logger.LogError($"Tried to submit a message from an {nameof(ActiveConvoViewModel)} whose {nameof(ActiveConvo)} is null! Something's wrong... please analyze this behaviour!");
                return false;
            }

            pageIndex = 0;

            await PullConvoMetadata();

            var encryptedMessagesBag = new ConcurrentBag<Tuple<string, string>>();
            string messageBodyJsonString = messageBodyJson.ToString(Formatting.None);

            // Get the keys of all convo participants here.
            List<Tuple<string, string>> keys = await userService.GetUserPublicKeyXml(user.Id, ActiveConvo.GetParticipantIdsCommaSeparated(), user.Token.Item2);

            // Encrypt the message for every convo participant individually
            // and put the result in a temporary concurrent bag.
            Parallel.ForEach(keys, key =>
            {
                if (key != null && key.Item1.NotNullNotEmpty() && key.Item2.NotNullNotEmpty() && messageBodyJsonString.NotNullNotEmpty())
                {
                    string encryptedMessage = crypto.EncryptMessage(messageBodyJsonString, RSAParametersExtensions.FromXmlString(key.Item2));
                    encryptedMessagesBag.Add(new Tuple<string, string>(key.Item1, encryptedMessage));
                }
            });

            var messageBodiesJson = new JObject();

            foreach (Tuple<string, string> encryptedMessage in encryptedMessagesBag)
            {
                messageBodiesJson[encryptedMessage.Item1] = encryptedMessage.Item2;
            }

            JToken ownMessageBody = messageBodiesJson[user.Id];
            if (ownMessageBody == null)
            {
                return false;
            }

            string username = settings["Username"];

            var postParamsDto = new PostMessageParamsDto
            {
                UserId = user.Id,
                SenderName = username,
                Auth = user.Token.Item2,
                ConvoPasswordSHA512 = convoPasswordProvider.GetPasswordSHA512(ActiveConvo.Id),
                MessageBodiesJson = messageBodiesJson.ToString(Formatting.None)
            };

            bool success = await convoService.PostMessage(ActiveConvo.Id, postParamsDto);
            return success;
        }

        /// <summary>
        /// Called when the user submitted a text message
        /// (either via UI button click or by pressing Enter).
        /// </summary>
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
                    ExecUI(() =>
                    {
                        var errorView = new InfoDialogView { DataContext = new InfoDialogViewModel { OkButtonText = "Okay :/", Text = "ERROR: Your text message couldn't be uploaded to the epistle Web API", Title = "Message upload failed" } };
                        errorView.ShowDialog();
                    });
                }
                messageBodyJson["text"] = messageBodyJson = null;
            });
        }

        /// <summary>
        /// Called when the user initiated a file upload via the UI.
        /// </summary>
        private void OnSendFile(object commandParam)
        {
            var dialog = new OpenFileDialog { Multiselect = false, Title = "Epistle - Select the file you want to send", InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) };
            dialog.FileOk += OnSelectedFile;
            dialog.ShowDialog();
        }

        /// <summary>
        /// Called when the user selected and confirmed
        /// the upload of an attachment file via the dialog box.
        /// </summary>
        /// <param name="sender">The origin <see cref="OpenFileDialog"/>.</param>
        /// <param name="e"><see cref="EventArgs"/></param>
        private void OnSelectedFile(object sender, EventArgs e)
        {
            var dialog = sender as OpenFileDialog;
            if (dialog is null || dialog.FileName.NullOrEmpty())
            {
                return;
            }

            OnDragAndDropFile(dialog.FileName);
        }

        /// <summary>
        /// Called when the user drag-n-dropped an attachment file.
        /// </summary>
        public void OnDragAndDropFile(string filePath)
        {
            if (filePath.NullOrEmpty())
            {
                return;
            }

            Task.Run(async () =>
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);

                if (fileBytes.LongLength < MAX_FILE_SIZE_BYTES)
                {
                    var messageBodyJson = new JObject
                    {
                        ["fileName"] = Path.GetFileName(filePath),
                        ["fileBase64"] = Convert.ToBase64String(fileBytes)
                    };

                    bool success = await SubmitMessage(messageBodyJson);

                    if (!success)
                    {
                        ExecUI(() =>
                        {
                            var errorView = new InfoDialogView { DataContext = new InfoDialogViewModel { OkButtonText = "Okay :/", Text = "ERROR: Your file couldn't be uploaded to the epistle Web API", Title = "Message upload failed" } };
                            errorView.ShowDialog();
                        });
                    }

                    messageBodyJson["fileBase64"] = messageBodyJson["fileName"] = messageBodyJson = null;
                }
                else
                {
                    ExecUI(() =>
                    {
                        var errorView = new InfoDialogView { DataContext = new InfoDialogViewModel { OkButtonText = "Okay :/", Text = "ERROR: Your file couldn't be uploaded to the epistle Web API because it exceeds the maximum file size of 20MB", Title = "Message upload failed" } };
                        errorView.ShowDialog();
                    });
                }
            });
        }

        /// <summary>
        /// Called when the user clicks on the
        /// scroll-to-bottom arrow button or presses escape.
        /// </summary>
        private void OnScrollToBottom(object commandParam)
        {
            ExecUI(TruncateMessagesCollection);
        }

        /// <summary>
        /// Called when the user clicked on the up-arrow button that loads the previous messages.
        /// </summary>
        private void OnClickedLoadPreviousMessages(object commandParam)
        {
            if (ActiveConvo is null || ActiveConvo.Id.NullOrEmpty())
            {
                return;
            }

            Task.Run(async () =>
            {
                var encryptedMessages = await messageRepository.GetLastMessages(MSG_COLLECTION_SIZE, MSG_COLLECTION_SIZE * ++pageIndex);
                var decryptedMessages = DecryptMessages(encryptedMessages).OrderBy(m => m.TimestampDateTimeUTC);

                var newCollection = Messages.ToList();
                newCollection.InsertRange(0, decryptedMessages);

                ExecUI(() =>
                {
                    Messages = new ObservableCollection<MessageViewModel>(newCollection);
                });
            });
        }

        /// <summary>
        /// User clicked on the copy convo id icon.
        /// </summary>
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

        /// <summary>
        /// Invoked when the user pressed escape whilst the message textbox was active.
        /// </summary>
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

        /// <summary>
        /// Invoked when the convo's metadata changed in some way
        /// (usually due to somebody joining the <see cref="Convo"/>
        /// or due to the admin changing title, description, etc...).
        /// </summary>
        private void OnChangedConvoMetadata(string convoId)
        {
            var convo = convoProvider[convoId];
            if (convo != null)
            {
                Name = convo.Name;
            }
        }

        /// <summary>
        /// Hides the green "copied" confirmation label.
        /// </summary>
        private void HideGreenTick()
        {
            ClipboardTickVisibility = Visibility.Hidden;
            scheduledHideGreenTickIcon = null;
        }
    }
}
