using System.Windows;
using System.Windows.Controls;

using GlitchedPolygons.GlitchedEpistle.Client.Windows.Extensions;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for UserExportDialog.xaml
    /// </summary>
    public partial class UserExportView : Window
    {
        public UserExportView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.MakeCloseable();
            Loaded -= OnLoaded;
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            Check();
        }

        private void PasswordBox2_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            Check();
        }

        private void OutputFilePathTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Check();
        }

        private void Check()
        {
            ExportButton.IsEnabled = PasswordBox.Password == PasswordBox2.Password && !string.IsNullOrEmpty(OutputFilePathTextBox.Text);
        }
    }
}
