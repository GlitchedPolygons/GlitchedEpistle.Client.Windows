using System.Windows;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Extensions;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for <c>ImportUserFromBackupView.xaml</c>.
    /// </summary>
    public partial class ImportUserFromBackupView : Window
    {
        public ImportUserFromBackupView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.MakeCloseable();
        }
    }
}
