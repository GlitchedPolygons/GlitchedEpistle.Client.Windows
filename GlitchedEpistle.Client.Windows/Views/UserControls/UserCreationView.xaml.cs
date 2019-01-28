using System.Windows.Controls;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.UserControls
{
    /// <summary>
    /// Interaction logic for RegisterView.xaml
    /// </summary>
    public partial class UserCreationView : UserControl
    {
        public UserCreationView()
        {
            InitializeComponent();
        }

        private void PasswordTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            RegisterButton.IsEnabled = CheckPassword();
        }

        private void PasswordTextBox2_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            RegisterButton.IsEnabled = CheckPassword();
        }

        private bool CheckPassword() => PasswordTextBox.Text == PasswordTextBox2.Text && PasswordTextBox.Text.Length > 7;
    }
}
