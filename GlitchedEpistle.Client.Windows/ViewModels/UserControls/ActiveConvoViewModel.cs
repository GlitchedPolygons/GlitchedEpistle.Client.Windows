using System;
using System.IO;
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
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class ActiveConvoViewModel : ViewModel
    {
        #region Constants
        public const long MAX_FILE_SIZE_MB = 20;

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

        private ObservableCollection<MessageViewModel> messages;
        public ObservableCollection<MessageViewModel> Messages { get => messages; set => Set(ref messages, value); }
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

                messageBodiesJson[key.Item1] = crypto.EncryptMessage(
                    messageJson: messageBodyJson.ToString(Formatting.None),
                    recipientPublicRsaKey: RSAParametersExtensions.FromXml(key.Item2)
                );
            }

            return await convoService.PostMessage(
                userId: user.Id,
                auth: user.Token.Item2,
                senderName: settings["Username"],
                convoId: ActiveConvo.Id,
                convoPasswordHash: ActiveConvo.PasswordSHA512,
                messageBodiesJson: messageBodiesJson.ToString(Formatting.None)
            );
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

                // TODO: add message to chatroom here
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

                    if (file.LongLength * 1000000L < MAX_FILE_SIZE_MB)
                    {
                        var messageBodyJson = new JObject
                        {
                            ["fileName"] = Path.GetFileName(_dialog.FileName),
                            ["fileBase64"] = Convert.ToBase64String(file)
                        };

                        if (await SubmitMessage(messageBodyJson))
                        {
                            // TODO: add message to chatroom here
                        }
                        else
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
