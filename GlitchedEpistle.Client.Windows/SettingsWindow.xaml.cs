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
        private bool cancelled = false;
        private readonly ISettings settings;

        public SettingsWindow(ISettings settings)
        {
            this.settings = settings;

            InitializeComponent();

            Load();
            Closed += SettingsWindow_Closed;
        }

        private void Save()
        {
            if (settings is null)
                return;

            settings["username"] = UsernameTextBox.Text;

            settings.Save();
        }

        private void Load()
        {
            if (settings is null)
                return;

            settings.Load();

            UsernameTextBox.Text = settings["username", "user"];
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

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            cancelled = true;
            Close();
        }

        private void RevertButton_OnClick(object sender, RoutedEventArgs e)
        {
            UsernameTextBox.Text = "user";
        }

        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            if (!cancelled)
            {
                Save();
            }

            Closed -= SettingsWindow_Closed;
        }
    }
}
