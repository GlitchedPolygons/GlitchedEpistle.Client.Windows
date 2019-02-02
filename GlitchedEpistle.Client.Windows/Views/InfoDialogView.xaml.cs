using System.Windows;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Extensions;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for InfoDialog.xaml.<para> </para>
    /// The InfoDialog is a simple, label-like informational dialog that only displays raw text with one button to dismiss the dialog. No further user interactivity is allowed!
    /// </summary>
    public partial class InfoDialogView : Window
    {
        public InfoDialogView()
        {
            InitializeComponent();
            Loaded += InfoDialogView_Loaded;
        }

        private void InfoDialogView_Loaded(object sender, RoutedEventArgs e)
        {
            this.MakeCloseable();
            Loaded -= InfoDialogView_Loaded;
        }
    }
}
