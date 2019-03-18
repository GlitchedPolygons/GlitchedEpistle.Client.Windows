using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Specialized;

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
                return;

            Border border = (Border)VisualTreeHelper.GetChild(MessagesListBox, 0);
            ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
            scrollViewer.ScrollToBottom();
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox.Focus();
        }
    }
}
