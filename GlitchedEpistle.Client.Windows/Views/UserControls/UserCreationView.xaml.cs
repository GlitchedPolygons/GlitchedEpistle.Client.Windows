using System.Windows;
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
            Loaded += OnLoaded;

            UsernameTextBox.GotFocus += (_, __) => UsernameTextBox.SelectAll();
            PasswordBox1.GotFocus += (_, __) => PasswordBox1.SelectAll();
            PasswordBox2.GotFocus += (_, __) => PasswordBox2.SelectAll();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            UsernameTextBox.Focus();
        }
    }
}
