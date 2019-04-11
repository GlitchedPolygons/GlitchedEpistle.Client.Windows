using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Specialized;
using System.Windows.Input;

using GlitchedPolygons.GlitchedEpistle.Client.Windows.ViewModels.UserControls;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.UserControls
{
    /// <summary>
    /// Interaction logic for ActiveConvoView.xaml
    /// </summary>
    public partial class ActiveConvoView : UserControl
    {
        private Border border;
        private ScrollViewer scrollViewer;

        public ActiveConvoView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            ((INotifyCollectionChanged)MessagesListBox.Items).CollectionChanged += ActiveConvoView_CollectionChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            TextBox.Focus();
            TextBox.SelectAll();

            if (VisualTreeHelper.GetChildrenCount(MessagesListBox) > 0)
            {
                border = (Border)VisualTreeHelper.GetChild(MessagesListBox, 0);
                scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                scrollViewer?.ScrollToBottom();
            }
        }

        private void ActiveConvoView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (AtBottom())
            {
                scrollViewer?.ScrollToBottom();
            }
        }

        private void ScrollToBottomButton_OnClick(object sender, RoutedEventArgs e)
        {
            scrollViewer?.ScrollToBottom();
        }

        private bool AtBottom()
        {
            if (scrollViewer is null) return false;
            return Math.Abs(scrollViewer.VerticalOffset - scrollViewer.ScrollableHeight) < 0.5d;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollToBottomButton.Visibility = AtBottom() ? Visibility.Hidden : Visibility.Visible;
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
            string filePath = draggedFiles[0];
            var dialog = new ConfirmFileUploadView(filePath);
            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value == true)
            {
                (DataContext as ActiveConvoViewModel)?.OnDragAndDropFile(filePath);
            }
            e.Handled = true;
        }

        private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                scrollViewer?.ScrollToBottom();
            }
        }

        private void SendTextButton_OnClick(object sender, RoutedEventArgs e)
        {
            scrollViewer?.ScrollToBottom();
        }
    }
}
