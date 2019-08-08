using System.Windows;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for ConfirmDeleteConvoLocallyView.xaml
    /// </summary>
    public partial class ConfirmDeleteConvoLocallyView : Window
    {
        public ConfirmDeleteConvoLocallyView()
        {
            InitializeComponent();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ConfirmDeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
