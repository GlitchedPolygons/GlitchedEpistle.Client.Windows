using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.UserControls
{
    /// <summary>
    /// Interaction logic for ActiveConvoView.xaml
    /// </summary>
    public partial class ActiveConvoView : UserControl
    {
        public ActiveConvoView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            ((INotifyCollectionChanged)MessagesListBox.Items).CollectionChanged += (_, __) => ScrollToBottom();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            TextBox.Focus();
            TextBox.SelectAll();
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            if (VisualTreeHelper.GetChildrenCount(MessagesListBox) <= 0)
            {
                return;
            }

            Border border = (Border)VisualTreeHelper.GetChild(MessagesListBox, 0);
            ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
            scrollViewer.ScrollToBottom();
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox.Focus();
        }

        private void TextBox_OnDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }
            string[] draggedFiles = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            e.Effects = draggedFiles is null || draggedFiles.Length != 1 ? DragDropEffects.None : DragDropEffects.Link;
            e.Handled = true;
        }

        private void TextBox_OnDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }
            string[] draggedFiles = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (draggedFiles is null || draggedFiles.Length != 1)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            (DataContext as ActiveConvoViewModel)?.OnDragAndDropFile(draggedFiles[0]);
            e.Handled = true;
        }
    }
}
