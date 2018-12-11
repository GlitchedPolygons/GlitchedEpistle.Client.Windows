using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly ISettings settings;

        public SettingsWindow(ISettings settings)
        {
            this.settings = settings;

            InitializeComponent();

            Load();
            Closed += SettingsWindow_Closed;
        }

        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            Save();
            Closed -= SettingsWindow_Closed;
        }

        private void Save()
        {
            settings?.Save();
        }

        private void Load()
        {
            if (settings is null)
                return;

            settings.Load();
            UsernameTextBox.Text = settings["username", "username"];
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

        private void UsernameTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: find a way to bind the textbox content to the variable without this code every time.
            if (sender is TextBox textBox)
            {
                settings["username"] = textBox.Text;
            }
            Save();
        }
    }
}
