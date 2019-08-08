using System.Windows;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for ConfirmLeaveConvoView.xaml
    /// </summary>
    public partial class ConfirmLeaveConvoView : Window
    {
        public ConfirmLeaveConvoView()
        {
            InitializeComponent();
        }

        private void LeaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
