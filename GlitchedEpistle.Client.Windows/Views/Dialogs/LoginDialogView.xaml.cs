using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for LoginDialogView.xaml
    /// </summary>
    public partial class LoginDialogView : Window
    {
        public LoginDialogView()
        {
            InitializeComponent();
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void UserIdTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            LoginButton.IsEnabled = FormReady;
        }

        private void PasswordTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            LoginButton.IsEnabled = FormReady;
        }

        private void PasswordTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DialogResult = true;
            }
        }

        private bool FormReady => !string.IsNullOrEmpty(UserIdTextBox.Text) && !string.IsNullOrEmpty(PasswordTextBox.Text);
    }
}
