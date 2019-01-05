using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

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

            // Immediately focus the username textbox
            // when the settings window is opened.
            Loaded += (sender, args) =>
            {
                UsernameTextBox?.Focus();
                UsernameTextBox?.SelectAll();
            };
        }

        // Select the text inside the username's textbox on click.
        private void UsernameTextBox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var textBox = sender as TextBox;

            if (textBox == null || textBox.IsKeyboardFocusWithin)
            {
                return;
            }

            e.Handled = true;
            textBox.Focus();
        }

        private void UsernameTextBox_SelectText(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox?.SelectAll();
        }
        
        private void UpdateFrequencySlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider)
            {
                UpdateFrequencyLabel.Content = $"Update Frequency ({slider.Value} ms)";
            }
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
