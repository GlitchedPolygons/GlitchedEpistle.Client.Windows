using System.Windows;
using System.Windows.Controls;

using GlitchedPolygons.GlitchedEpistle.Client.Extensions;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.UserControls
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            UserIdTextBox.GotFocus += (_, __) => UserIdTextBox.SelectAll();
            PasswordBox.GotFocus += (_, __) => PasswordBox.SelectAll();
            TotpTextBox.GotFocus += (_, __) => TotpTextBox.SelectAll();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (UserIdTextBox.Text.NullOrEmpty())
            {
                UserIdTextBox.Focus();
            }
            else
            {
                PasswordBox.Focus();
            }
            RegisterButton.IsEnabled = UserIdTextBox.Text.NullOrEmpty();
        }

        private void UserIdTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            LoginButton.IsEnabled = FormReady;
            RegisterButton.IsEnabled = UserIdTextBox.Text.NullOrEmpty();
        }

        private void PasswordTextBox_OnTextChanged(object sender, RoutedEventArgs e)
        {
            LoginButton.IsEnabled = FormReady;
        }

        private void TotpTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            LoginButton.IsEnabled = FormReady;
        }

        private bool FormReady => !string.IsNullOrEmpty(UserIdTextBox.Text) && !string.IsNullOrEmpty(PasswordBox.Password) && !string.IsNullOrEmpty(TotpTextBox.Text);
    }
}
