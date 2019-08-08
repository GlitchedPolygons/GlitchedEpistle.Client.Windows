using System.Windows;

using GlitchedPolygons.GlitchedEpistle.Client.Windows.Extensions;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for EditConvoMetadataView.xaml
    /// </summary>
    public partial class EditConvoMetadataView : Window
    {
        public EditConvoMetadataView()
        {
            InitializeComponent();
            Loaded += EditConvoMetadataView_Loaded;
        }

        private void EditConvoMetadataView_Loaded(object sender, RoutedEventArgs e)
        {
            this.MakeCloseable();
        }
    }
}
