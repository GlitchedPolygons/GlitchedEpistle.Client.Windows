using System;
using System.IO;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using GlitchedPolygons.Services.MethodQ;
using GlitchedPolygons.GlitchedEpistle.Client.Extensions;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Views;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands;

using Microsoft.Win32;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls
{
    public class MessageViewModel : ViewModel
    {
        #region Constants
        // Injections:
        private readonly IMethodQ methodQ;
        #endregion

        #region Commands
        public ICommand DownloadAttachmentCommand { get; }
        public ICommand CopyUserIdToClipboardCommand { get; }
        public ICommand ClickedOnImageAttachmentCommand { get; }
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

        private byte[] fileBytes;
        public byte[] FileBytes
        {
            get => fileBytes;
            set
            {
                fileBytes = value;
                if (value != null)
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.DecodePixelWidth = 300;
                    img.StreamSource = new MemoryStream(value);
                    img.EndInit();
                    img.Freeze();
                    Image = img;
                }
            }
        }

        public string FileSize => $"({FileBytes.GetFileSizeString()})";

        public BitmapImage Image { get; set; }

        private string timestamp = string.Empty;
        public string Timestamp { get => timestamp; set => Set(ref timestamp, value); }

        public DateTime TimestampDateTimeUTC { get; set; }

        private Visibility clipboardTickVisibility = Visibility.Hidden;
        public Visibility ClipboardTickVisibility { get => clipboardTickVisibility; set => Set(ref clipboardTickVisibility, value); }

        public Visibility ImageVisibility => IsImage() ? Visibility.Visible : Visibility.Hidden;
        public Visibility AttachmentButtonVisibility => HasAttachment() ? Visibility.Visible : Visibility.Hidden;
        #endregion

        private string id = null;
        /// <summary>
        /// Gets the message's unique identifier, which is <para> </para>
        /// md5( <see cref="SenderId"/> + UTC timestamp )
        /// </summary>
        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(id))
                {
                    id = (SenderId + TimestampDateTimeUTC.ToString("u")).MD5();
                }
                return id;
            }
        }

        private ulong? scheduledHideGreenTickIcon = null;

        public MessageViewModel(IMethodQ methodQ)
        {
            this.methodQ = methodQ;

            DownloadAttachmentCommand = new DelegateCommand(OnDownloadAttachment);
            CopyUserIdToClipboardCommand = new DelegateCommand(OnCopyUserIdToClipboard);
            ClickedOnImageAttachmentCommand = new DelegateCommand(OnClickedImagePreview);
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

        private void OnDownloadAttachment(object commandParam)
        {
            string ext = Path.GetExtension(FileName) ?? string.Empty;

            var dialog = new SaveFileDialog
            {
                Title = "Download attachment",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                FileName = FileName,
                DefaultExt = ext,
                AddExtension = true,
                OverwritePrompt = true,
                Filter = $"Epistle Message Attachment|*{ext}"
            };

            dialog.FileOk += (sender, e) =>
            {
                if (sender is SaveFileDialog _dialog)
                {
                    File.WriteAllBytes(_dialog.FileName, FileBytes);
                }
            };

            dialog.ShowDialog();
        }

        private void OnClickedImagePreview(object commandParam)
        {
            var viewModel = new ImageViewerViewModel { ImageBytes = FileBytes };
            var view = new ImageViewerView { DataContext = viewModel };
            view.ShowDialog();
        }

        private void OnCopyUserIdToClipboard(object commandParam)
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
        }

        private bool HasAttachment()
        {
            return FileName.NotNullNotEmpty() && FileBytes != null && FileBytes.Length > 0;
        }

        private bool IsImage()
        {
            if (!HasAttachment())
            {
                return false;
            }

            return FileName.EndsWith(".png", true, CultureInfo.InvariantCulture)
                   || FileName.EndsWith(".jpg", true, CultureInfo.InvariantCulture)
                   || FileName.EndsWith(".jpeg", true, CultureInfo.InvariantCulture)
                   || FileName.EndsWith(".tif", true, CultureInfo.InvariantCulture);
        }
    }
}
