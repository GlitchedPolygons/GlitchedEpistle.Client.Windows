using System.Windows;
using System.Windows.Controls;

using GlitchedPolygons.GlitchedEpistle.Client.Windows.Extensions;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for <c>JoinConvoDialogView.xaml</c>
    /// </summary>
    public partial class JoinConvoDialogView : Window
    {
        public JoinConvoDialogView()
        {
            InitializeComponent();
            ConvoIdTextBox.Focus();
            ConvoIdTextBox.SelectAll();
            Loaded += OnLoaded;
            ConvoIdTextBox.TextChanged += ConvoIdTextBox_TextChanged;
        }

        private void ConvoIdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(ConvoIdTextBox.Text))
            {
                ConvoIdTextBox.Focus();
                ConvoIdTextBox.SelectAll();
            }
            else
            {
                ConvoPasswordBox.Focus();
                ConvoPasswordBox.SelectAll();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.MakeCloseable();
            this.Loaded -= OnLoaded;
        }
    }
}
