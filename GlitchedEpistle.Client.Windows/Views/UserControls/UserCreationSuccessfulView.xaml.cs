using System.Windows.Controls;

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
        }

        private void TotpTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            VerifyButton.IsEnabled = TotpTextBox.Text.Length == 6;
        }
    }
}
