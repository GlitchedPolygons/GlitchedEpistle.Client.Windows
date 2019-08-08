using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.UserControls
{
    /// <summary>
    /// Interaction logic for RegistrationSuccessfulView.xaml
    /// </summary>
    public partial class UserCreationSuccessfulView : UserControl
    {
        public UserCreationSuccessfulView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            // Immediately focus the 2FA secret textbox when the control is loaded.
            SecretTextBox?.Focus();
            SecretTextBox?.SelectAll();
        }

        private void TotpTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            VerifyButton.IsEnabled = TotpTextBox.Text.Length == 6;
        }

        private void SecretTextBox_SelectText(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox?.SelectAll();
        }

        private void SecretTextBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox == null || textBox.IsKeyboardFocusWithin)
            {
                return;
            }

            e.Handled = true;
            textBox.Focus();
        }
    }
}
