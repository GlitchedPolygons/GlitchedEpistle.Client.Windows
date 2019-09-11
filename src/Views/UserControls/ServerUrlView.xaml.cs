using System.Windows.Controls;
using GlitchedPolygons.ExtensionMethods;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.UserControls
{
    /// <summary>
    /// Interaction logic for ServerUrlView.xaml
    /// </summary>
    public partial class ServerUrlView : UserControl
    {
        public ServerUrlView()
        {
            InitializeComponent();
        }

        private void ServerUrlTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TestConnectionButton.IsEnabled =
                ServerUrlTextBox.Text.NotNullNotEmpty() 
                && ServerUrlTextBox.Text.Contains(".")
                && (ServerUrlTextBox.Text.Contains("http://") || ServerUrlTextBox.Text.Contains("https://"));
        }
    }
}
