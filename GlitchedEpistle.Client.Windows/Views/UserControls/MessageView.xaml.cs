using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.UserControls
{
    /// <summary>
    /// Interaction logic for MessageView.xaml
    /// </summary>
    public partial class MessageView : UserControl
    {
        public MessageView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            TextBox.TextChanged += TextBox_TextChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            TextBox_TextChanged(null, null);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox.Visibility = string.IsNullOrEmpty(TextBox.Text) ? Visibility.Hidden : Visibility.Visible;
        }

        // Select the text inside the textbox on click.
        private void TextBox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null || textBox.IsKeyboardFocusWithin)
            {
                return;
            }

            e.Handled = true;
            textBox.Focus();
        }

        private void TextBox_SelectText(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox?.SelectAll();
        }
    }
}
