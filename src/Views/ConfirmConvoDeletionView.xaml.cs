using System.Windows;
using System.Windows.Controls;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for ConfirmConvoDeletionView.xaml
    /// </summary>
    public partial class ConfirmConvoDeletionView : Window
    {
        public ConfirmConvoDeletionView()
        {
            InitializeComponent();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            DeleteButton.IsEnabled = TextBox.Text.Equals("DELETE!");
        }
    }
}
