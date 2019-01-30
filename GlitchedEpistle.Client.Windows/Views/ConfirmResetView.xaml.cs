using System.Windows;
using System.Windows.Controls;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for ConfirmResetView.xaml
    /// </summary>
    public partial class ConfirmResetView : Window
    {
        public ConfirmResetView()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                ConfirmButton.IsEnabled = textBox.Text == "DELETE!";
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
