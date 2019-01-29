using System.Windows;
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
    }
}
