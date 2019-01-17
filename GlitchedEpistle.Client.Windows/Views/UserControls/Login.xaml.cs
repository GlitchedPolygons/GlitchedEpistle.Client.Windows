using System.Windows.Controls;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.UserControls
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();
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
