/*
    Glitched Epistle - Windows Client
    Copyright (C) 2020 Raphael Beck

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

#define PARALLEL_LOAD
// Comment out the above line to load/decrypt messages synchronously.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

using GlitchedPolygons.RepositoryPattern;
using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.Services.MethodQ;
using GlitchedPolygons.Services.Cryptography.Symmetric;
using GlitchedPolygons.Services.Cryptography.Asymmetric;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Web.Convos;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Localization;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.PubSubEvents;

using Microsoft.Win32;

using Prism.Events;
using GlitchedPolygons.Services.CompressionUtility;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    /// <summary>
    /// This is one of the most important classes inside the Windows client code.<para> </para>
    /// It contains the view model for the currently active convo, meaning that it will
    /// take care of pulling the newest messages from the backend, adding them to the UI, etc...
    /// </summary>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public class ActiveConvoViewModel : ViewModel, IDisposable
    {
        #region Constants
        private const int MSG_COLLECTION_SIZE = 20;
        private const string MSG_TIMESTAMP_FORMAT = "dd.MM.yyyy HH:mm";
        private static readonly TimeSpan METADATA_PULL_FREQUENCY = TimeSpan.FromMilliseconds(30000);

        // Injections:
        private readonly User user;
        private readonly ILogger logger;
        private readonly IMethodQ methodQ;
        private readonly ISymmetricCryptography aes;
        private readonly IAsymmetricCryptographyRSA rsa;
        private readonly ILocalization localization;
        private readonly IConvoService convoService;
        private readonly IMessageSender messageSender;
        private readonly IMessageFetcher messageFetcher;
        private readonly IEventAggregator eventAggregator;
        private readonly ICompressionUtility compressionUtility;
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
                long? exp = ActiveConvo?.ExpirationUTC;
                if (exp.HasValue)
                {
                    if (DateTime.UtcNow.ToUnixTimeMilliseconds() > exp.Value)
                    {
                        value += " (EXPIRED)";
                    }
                    else if ((exp.Value.FromUnixTimeMilliseconds() - DateTime.UtcNow).TotalDays < 3)
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

        private volatile bool disposed;
        private volatile int pageIndex;
        private volatile CancellationTokenSource autoFetch;
        private volatile CancellationTokenSource metadataUpdater;

        private ulong? scheduledHideGreenTickIcon;
        private IMessageRepository messageRepository;

        private Convo activeConvo;
        public Convo ActiveConvo
        {
            get => activeConvo;
            set
            {
                activeConvo = value;
                Name = value.Name;
            }
        }

        public ActiveConvoViewModel(
            User user,
            ILogger logger,
            IMethodQ methodQ,
            ILocalization localization,
            IConvoService convoService,
            IEventAggregator eventAggregator,
            ISymmetricCryptography aes,
            IAsymmetricCryptographyRSA rsa,
            IConvoPasswordProvider convoPasswordProvider,
            ICompressionUtility compressionUtility,
            IMessageSender messageSender,
            IMessageFetcher messageFetcher)
        {
            #region Injections
            this.aes = aes;
            this.rsa = rsa;
            this.user = user;
            this.logger = logger;
            this.methodQ = methodQ;
            this.convoService = convoService;
            this.localization = localization;
            this.compressionUtility = compressionUtility;
            this.convoPasswordProvider = convoPasswordProvider;
            this.eventAggregator = eventAggregator;
            this.messageSender = messageSender;
            this.messageFetcher = messageFetcher;
            #endregion

            convoProvider = new ConvoRepositorySQLite($"Data Source={Path.Combine(Paths.GetConvosDirectory(user.Id), "_metadata.db")};Version=3;");

            SendTextCommand = new DelegateCommand(OnSendText);
            SendFileCommand = new DelegateCommand(OnSendFile);
            PressedEscapeCommand = new DelegateCommand(OnPressedEscape);
            ScrollToBottomCommand = new DelegateCommand(OnScrollToBottom);
            LoadPreviousMessagesCommand = new DelegateCommand(OnClickedLoadPreviousMessages);
            CopyConvoIdToClipboardCommand = new DelegateCommand(OnClickedCopyConvoIdToClipboard);

            eventAggregator.GetEvent<LogoutEvent>().Subscribe(Dispose);
            eventAggregator.GetEvent<ChangedConvoMetadataEvent>().Subscribe(OnChangedConvoMetadata);

            // When switching to another convo, dispose this viewmodel.
            // The subscribed Dispose method should stop the automatic pulling.
            // Last thing you'd wanna have to debug is some background thread trying to pull from a convo that you already closed...
            eventAggregator.GetEvent<JoinedConvoEvent>().Subscribe(_ => Dispose());
        }

        public void Init()
        {
            if (ActiveConvo is null || ActiveConvo.Id.NullOrEmpty())
            {
                throw new NullReferenceException($"{nameof(ActiveConvoViewModel)}::{nameof(Init)}: Tried to initialize a convo viewmodel without assigning it an {nameof(ActiveConvo)} first. Please assign that before calling init.");
            }

            // Prevent the user from both pulling and
            // submitting new messages whilst changing convos.
            CanSend = false;
            StopAutomaticPulling();

            Messages = new ObservableCollection<MessageViewModel>();
            messageRepository = new MessageRepositorySQLite($"Data Source={Path.Combine(Paths.GetConvosDirectory(user.Id), ActiveConvo.Id + ".db")};Version=3;");

            // Decrypt the messages that are already stored 
            // locally on the device and load them into the view.
            // Then, resume the user's ability to send messages once done.
            Task.Run(async () =>
            {
                var encryptedMessages = await messageRepository.GetLastMessages(MSG_COLLECTION_SIZE).ConfigureAwait(false);

                var decryptedMessages = DecryptMessages(encryptedMessages).Where(m => m != null).Distinct().OrderBy(m => m.TimestampDateTimeUTC);
                
                ExecUI(() =>
                {
                    Messages.AddRange(decryptedMessages);
                    StartAutomaticPulling();
                    CanSend = true;
                });
            });
        }

        ~ActiveConvoViewModel()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            StopAutomaticPulling();
        }

        /// <summary>
        /// Stops the <see cref="ActiveConvoViewModel"/> from automatically pulling messages.
        /// </summary>
        private void StopAutomaticPulling()
        {
            autoFetch?.Cancel();
            autoFetch = null;

            metadataUpdater?.Cancel();
            metadataUpdater = null;
        }

        /// <summary>
        /// Starts the automatic message pull cycle.
        /// </summary>
        private async void StartAutomaticPulling()
        {
            StopAutomaticPulling();

            autoFetch = messageFetcher.StartAutoFetchingMessages(
                ActiveConvo.Id,
                convoPasswordProvider.GetPasswordSHA512(ActiveConvo.Id),
                await messageRepository.GetLastMessageId(),
                OnFetchedNewMessages
            );

            metadataUpdater = new CancellationTokenSource();

            var task = Task.Run(async () =>
            {
                while (!metadataUpdater.IsCancellationRequested)
                {
                    await PullConvoMetadata().ConfigureAwait(false);
                    await Task.Delay(METADATA_PULL_FREQUENCY).ConfigureAwait(false);
                }
            }, metadataUpdater.Token);
        }

        /// <summary>
        /// Callback method for the auto-fetching routine.<para> </para>
        /// Gets called when there were new messages in the <see cref="Convo"/> server-side.<para> </para>
        /// Also truncates the view's message collection size when needed.
        /// </summary>
        /// <param name="fetchedMessages">The messages that were fetched from the backend.</param>
        private void OnFetchedNewMessages(IEnumerable<Message> fetchedMessages)
        {
            if (fetchedMessages is null)
            {
                return;
            }
            
            // Add the pulled messages to the local sqlite db.
            messageRepository.AddRange(fetchedMessages.OrderBy(m => m.TimestampUTC)).ContinueWith(async result =>
            {
                if (await result == false)
                {
                    logger.LogError($"{nameof(ActiveConvoViewModel)}::<<AutomaticPullCycle>>: ConvoId={ActiveConvo?.Id}  >> The retrieved messages (from message id {fetchedMessages.First()?.Id} onwards) could not be added to the local sqlite db on disk. Reason unknown...");
                }
            });

            // Decrypt and add the retrieved messages to the chatroom UI.
            var decryptedMessages = DecryptMessages(fetchedMessages).Where(m => m != null).Distinct().OrderBy(m => m.TimestampDateTimeUTC);
            
            ExecUI(delegate
            {
                Messages.AddRange(decryptedMessages);

                if (pageIndex == 0)
                {
                    TruncateMessagesCollection();
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

            try
            {
                var messageViewModel = new MessageViewModel(methodQ)
                {
                    Id = message.Id.ToString(),
                    SenderId = message.SenderId,
                    SenderName = message.SenderName,
                    IsFromServer = message.IsFromServer(),
                    TimestampDateTimeUTC = message.TimestampUTC.FromUnixTimeMilliseconds(),
                    Timestamp = message.TimestampUTC.FromUnixTimeMilliseconds().ToLocalTime().ToString(MSG_TIMESTAMP_FORMAT),
                    IsOwn = message.SenderId.Equals(user.Id),
                };

                if (message.IsFromServer())
                {
                    string[] split = message.EncryptedBody.Split(':');
                    if (split.Length != 3 || !int.TryParse(split[1], out int messageType))
                    {
                        logger.LogError($"{nameof(ActiveConvoViewModel)}::{nameof(DecryptMessage)}: Broadcast message from the backend was submitted to the convo '{activeConvo.Id}' in an invalid format: was the server compromised?!");
                        return null;
                    }
                    switch (messageType)
                    {
                        case 0:
                            messageViewModel.Text = string.Format(localization["UserJoinedConvo"], split[2]);
                            break;
                        case 1:
                            messageViewModel.Text = string.Format(localization["UserLeftConvo"], split[2]);
                            break;
                        case 2:
                            messageViewModel.Text = string.Format(localization["UserWasKickedFromConvo"], split[2]);
                            break;
                        case 3:
                            messageViewModel.Text = localization["ConvoAboutToExpire"];
                            break;
                        case 4:
                            if (!int.TryParse(split[2], out int change))
                            {
                                logger.LogError($"{nameof(ActiveConvoViewModel)}::{nameof(DecryptMessage)}: Broadcast message from the backend was submitted to the convo '{activeConvo.Id}' in an invalid format: was the server compromised?!");
                                return null;
                            }
                            
                            var sb = new StringBuilder(localization["ConvoMetadataChanged"]).Append(' ');

                            if ((change & 1 << 0) > 0)
                            {
                                sb.Append(localization["ConvoAdmin"]).Append(", ");
                            }
                            if ((change & 1 << 1) > 0)
                            {
                                sb.Append(localization["ConvoTitle"]).Append(", ");
                            }
                            if ((change & 1 << 2) > 0)
                            {
                                sb.Append(localization["ConvoDescription"]).Append(", ");
                            }
                            if ((change & 1 << 3) > 0)
                            {
                                sb.Append(localization["ConvoExpiration"]).Append(", ");
                            }
                            if ((change & 1 << 4) > 0)
                            {
                                sb.Append(localization["ConvoPassword"]).Append(", ");
                            }
                            
                            sb.Length -= 2;
                            messageViewModel.Text = sb.ToString();
                            break;
                    }
                    messageViewModel.FileName = null;
                    messageViewModel.FileBytes = null;
                }
                else
                {
                    if (message.EncryptedKey.NullOrEmpty())
                    {
                        return null;
                    }

                    byte[] decryptedKey = rsa.Decrypt(Convert.FromBase64String(message.EncryptedKey), user.PrivateKeyPem);

                    var encryptedMessage = new EncryptionResult 
                    { 
                        Key = decryptedKey.Take(32).ToArray(), 
                        IV = decryptedKey.Skip(32).ToArray(), 
                        EncryptedData = Convert.FromBase64String(message.EncryptedBody) 
                    };

                    byte[] decryptedMessage = compressionUtility.Decompress(aes.Decrypt(encryptedMessage), CompressionSettings.Default);
                    
                    if (message.Type.StartsWith("TEXT="))
                    {
                        Encoding encoding;
                        switch (message.Type.Substring(5))
                        {
                            default:
                            case "UTF8":
                                encoding = Encoding.UTF8;
                                break;
                            case "UTF7":
                                encoding = Encoding.UTF7;
                                break;
                            case "UTF32":
                                encoding = Encoding.UTF32;
                                break;
                            case "ASCII":
                                encoding = Encoding.ASCII;
                                break;
                            case "Unicode":
                                encoding = Encoding.Unicode;
                                break;
                        }
                        messageViewModel.Text = encoding.GetString(decryptedMessage);
                    }
                    else if (message.Type.StartsWith("FILE="))
                    {
                        messageViewModel.FileName = message.Type.Substring(5);
                        messageViewModel.FileBytes = decryptedMessage;
                    }
                    else
                    {
                        logger?.LogError($"{nameof(ActiveConvoViewModel)}::{nameof(DecryptMessage)}: Decryption succeeded but message has invalid format!");
                        return null;
                    }
                }

                return messageViewModel;
            }
            catch
            {
                return null;
            }
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
            convo.CreationUTC = ActiveConvo.CreationUTC = metadataDto.CreationUTC;
            convo.BannedUsers = ActiveConvo.BannedUsers = metadataDto.BannedUsers.Split(',').ToList();
            convo.Participants = ActiveConvo.Participants = metadataDto.Participants.Split(',').ToList();

            eventAggregator.GetEvent<ChangedConvoMetadataEvent>().Publish(convo.Id);

            return await convoProvider.Update(convo);
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
                bool success = await messageSender.PostText(ActiveConvo, messageText);

                if (!success)
                {
                    ExecUI(() =>
                    {
                        new InfoDialogView
                        {
                            DataContext = new InfoDialogViewModel
                            {
                                OkButtonText = "Okay :/",
                                Title = localization["MessageUploadFailed"],
                                Text = localization["TextMessageCouldNotBeUploaded"]
                            }
                        }.ShowDialog();
                    });
                }
            });
        }

        /// <summary>
        /// Called when the user initiated a file upload via the UI.
        /// </summary>
        private void OnSendFile(object commandParam)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = localization["SelectTheFileYouWantToSend"],
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
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

                if (fileBytes.LongLength < MessageSender.MAX_FILE_SIZE_BYTES)
                {
                    bool success = await messageSender.PostFile(ActiveConvo, Path.GetFileName(filePath), fileBytes);

                    if (!success)
                    {
                        ExecUI(() =>
                        {
                            new InfoDialogView
                            {
                                DataContext = new InfoDialogViewModel
                                {
                                    OkButtonText = "Okay :/",
                                    Title = localization["MessageUploadFailed"],
                                    Text = localization["FileCouldNotBeUploaded"]
                                }
                            }.ShowDialog();
                        });
                    }
                }
                else
                {
                    ExecUI(() =>
                    {
                        new InfoDialogView
                        {
                            DataContext = new InfoDialogViewModel
                            {
                                OkButtonText = "Okay :/",
                                Title = localization["MessageUploadFailed"],
                                Text = localization["FileCouldNotBeUploadedBecauseItExceedsMaxFileSizeLimit"]
                            }
                        }.ShowDialog();
                    });
                }
            });
        }

        public async void PasteImage()
        {
            var img = Clipboard.GetImage();
            if (img is null)
            {
                return;
            }

            byte[] fileBytes;
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                encoder.Save(stream);
                fileBytes = stream.ToArray();
            }

            if (fileBytes.LongLength >= MessageSender.MAX_FILE_SIZE_BYTES)
            {
                new InfoDialogView
                {
                    DataContext = new InfoDialogViewModel
                    {
                        OkButtonText = "Okay :/",
                        Title = localization["MessageUploadFailed"],
                        Text = localization["FileCouldNotBeUploadedBecauseItExceedsMaxFileSizeLimit"]
                    }
                }.ShowDialog();
                return;
            }

            var dialog = new ConfirmFileUploadView('{' + localization["CLIPBOARD"] + '}');
            bool? dialogResult = dialog.ShowDialog();
            if (!dialogResult.HasValue || dialogResult.Value != true)
            {
                return;
            }

            bool success = await messageSender.PostFile(ActiveConvo, $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.png", fileBytes);
            if (!success)
            {
                new InfoDialogView
                {
                    DataContext = new InfoDialogViewModel
                    {
                        OkButtonText = "Okay :/",
                        Title = localization["MessageUploadFailed"],
                        Text = localization["FileCouldNotBeUploaded"]
                    }
                }.ShowDialog();
            }
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
                var encryptedMessages = await messageRepository.GetLastMessages(MSG_COLLECTION_SIZE, MSG_COLLECTION_SIZE * ++pageIndex).ConfigureAwait(false);
                var decryptedMessages = DecryptMessages(encryptedMessages).OrderBy(m => m.Id);

                var collection = Messages.ToList();
                collection.InsertRange(0, decryptedMessages);

                var newCollection = new ObservableCollection<MessageViewModel>(collection);
                ExecUI(() => Messages = newCollection);
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
                StopAutomaticPulling();
                StartAutomaticPulling();
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
