using System.Windows;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for ConfirmKickUserView.xaml
    /// </summary>
    public partial class ConfirmKickUserView : Window
    {
        public ConfirmKickUserView()
        {
            InitializeComponent();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
