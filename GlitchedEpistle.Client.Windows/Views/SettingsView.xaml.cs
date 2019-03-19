using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using GlitchedPolygons.GlitchedEpistle.Client.Windows.Extensions;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Views
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsView : Window
    {
        public SettingsView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            // Immediately focus the username textbox
            // when the settings window is opened.
            UsernameTextBox?.Focus();
            UsernameTextBox?.SelectAll();

            // Subscribe to the ICloseable.RequestedClose event
            // to close this view when requested to by the ViewModel.
            this.MakeCloseable();

            RedeemCouponButton.IsEnabled = !string.IsNullOrEmpty(CouponTextBox.Text);
        }

        // Select the text inside the username's textbox on click.
        private void UsernameTextBox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox == null || textBox.IsKeyboardFocusWithin)
            {
                return;
            }

            e.Handled = true;
            textBox.Focus();
        }

        private void UsernameTextBox_SelectText(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox?.SelectAll();
        }

        private void CouponTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                RedeemCouponButton.IsEnabled = !string.IsNullOrEmpty(textBox.Text);
            }
        }
    }
}
