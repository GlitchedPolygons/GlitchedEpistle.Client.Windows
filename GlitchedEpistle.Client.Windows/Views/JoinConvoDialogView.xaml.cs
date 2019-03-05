using System.Windows;
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
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= OnLoaded;
            this.MakeCloseable();
        }
    }
}
