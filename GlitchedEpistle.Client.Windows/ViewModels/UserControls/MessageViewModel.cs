using System;
using System.Windows;
using System.Windows.Input;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;
using GlitchedPolygons.Services.MethodQ;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class MessageViewModel : ViewModel
    {
        #region Constants
        // Injections:
        private readonly IMethodQ methodQ;
        #endregion

        #region Commands
        public ICommand DownloadFileCommand { get; }
        public ICommand CopyUserIdToClipboardCommand { get; }
        #endregion

        #region UI Bindings
        private string senderId = string.Empty;
        public string SenderId { get => senderId; set => Set(ref senderId, value); }

        private string senderName = string.Empty;
        public string SenderName { get => senderName; set => Set(ref senderName, value); }

        private string text = string.Empty;
        public string Text { get => text; set => Set(ref text, value); }

        private string fileName = string.Empty;
        public string FileName { get => fileName; set => Set(ref fileName, value); }

        private string timestamp = string.Empty;
        public string Timestamp { get => timestamp; set => Set(ref timestamp, value); }

        private Visibility clipboardTickVisibility = Visibility.Hidden;
        public Visibility ClipboardTickVisibility { get => clipboardTickVisibility; set => Set(ref clipboardTickVisibility, value); }
        #endregion

        public byte[] FileBytes { get; set; }
        public DateTime TimestampDateTime { get; set; }

        private ulong? scheduledHideGreenTickIcon = null;

        private string id = null;
        /// <summary>
        /// Gets the message's unique identifier, which is <para> </para>
        /// md5( <see cref="SenderId"/> + <see cref="Timestamp"/> )
        /// </summary>
        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(id))
                {
                    id = (SenderId + TimestampDateTime.ToString("u")).MD5();
                }
                return id;
            }
        }

        public MessageViewModel(IMethodQ methodQ)
        {
            this.methodQ = methodQ;

            CopyUserIdToClipboardCommand = new DelegateCommand(_ =>
            {
                Clipboard.SetText(SenderId);
                ClipboardTickVisibility = Visibility.Visible;

                if (scheduledHideGreenTickIcon.HasValue)
                    methodQ.Cancel(scheduledHideGreenTickIcon.Value);

                scheduledHideGreenTickIcon = methodQ.Schedule(() =>
                {
                    ClipboardTickVisibility = Visibility.Hidden;
                    scheduledHideGreenTickIcon = null;
                }, DateTime.UtcNow.AddSeconds(3));
            });

            DownloadFileCommand = new DelegateCommand(_ =>
            {
                // TODO: open savefile dialog here 
            });
        }

        ~MessageViewModel()
        {
            Text = FileName = null;
            if (FileBytes != null)
            {
                for (int i = 0; i < FileBytes.Length; i++)
                {
                    FileBytes[i] = 0;
                }
            }
        }
    }
}
