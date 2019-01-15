using System.Windows;
using System.Windows.Controls;

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
            Closing += (sender, args) => Application.Current.Shutdown();
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

        private bool FormReady => !string.IsNullOrEmpty(UserIdTextBox.Text) && !string.IsNullOrEmpty(PasswordTextBox.Text);
    }
}
