/*
    Glitched Epistle - Windows Client
    Copyright (C) 2019 Raphael Beck

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

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Specialized;

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
        private MessageViewModel lastItem;
        private bool loadingPrevMsgs = false;

        public ActiveConvoView()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            MessagesListBox.SizeChanged += ActiveConvoView_SizeChanged;
            ((INotifyCollectionChanged)MessagesListBox.Items).CollectionChanged += ActiveConvoView_CollectionChanged;
        }

        private void ActiveConvoView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MessagesListBox.Dispatcher.Invoke(MessagesListBox.UpdateLayout);
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
            else
            {
                if (loadingPrevMsgs)
                {
                    loadingPrevMsgs = false;
                    MessagesListBox.ScrollIntoView(lastItem);
                }
            }
        }

        private void ScrollToBottomButton_OnClick(object sender, RoutedEventArgs e)
        {
            scrollViewer?.ScrollToBottom();
        }

        private void LoadPreviousMessagesButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessagesListBox.HasItems)
            {
                loadingPrevMsgs = true;
                lastItem = MessagesListBox?.Items?[0] as MessageViewModel;
            }
        }

        private bool AtBottom()
        {
            if (scrollViewer is null) return false;
            return Math.Abs(scrollViewer.VerticalOffset - scrollViewer.ScrollableHeight) < 0.75d;
        }

        private bool AtTop()
        {
            if (scrollViewer is null) return false;
            return Math.Abs(scrollViewer.VerticalOffset) < 0.01d;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollToBottomButton.Visibility = AtBottom() ? Visibility.Hidden : Visibility.Visible;
            LoadPreviousMessagesButton.Visibility = AtTop() ? Visibility.Visible : Visibility.Hidden;
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
    }
}
