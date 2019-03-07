using System.Windows;
using System.Windows.Controls;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views.UserControls
{
    /// <summary>
    /// Interaction logic for ActiveConvoView.xaml
    /// </summary>
    public partial class ActiveConvoView : UserControl
    {
        public ActiveConvoView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            TextBox.Focus();
        }
    }
}
