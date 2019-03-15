using System.Windows;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Extensions;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for ChangePasswordView.xaml
    /// </summary>
    public partial class ChangePasswordView : Window
    {
        public ChangePasswordView()
        {
            InitializeComponent();
            this.Loaded += ChangePasswordView_Loaded;
        }

        private void ChangePasswordView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= ChangePasswordView_Loaded;
            this.MakeCloseable();
            OldPasswordBox.Focus();
        }
    }
}
