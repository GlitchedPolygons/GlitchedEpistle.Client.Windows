using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings;
using Newtonsoft.Json;
using Path = System.IO.Path;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly ILogger logger;
        private readonly ISettings settings;
        
        public SettingsWindow(ISettings settings, ILogger logger)
        {
            this.logger = logger;
            this.settings = settings;

            InitializeComponent();

            Load();
            Closed += Settings_Closed;
        }

        private void Settings_Closed(object sender, EventArgs e)
        {
            Save();
            Closed -= Settings_Closed;
        }
        
        private void Save()
        {

            settings?.Save();
        }

        private void Load()
        {
            //settings = JsonConvert.DeserializeObject<SettingsData>(File.ReadAllText(FilePath));

            //UsernameTextBox.Text = settings.username;
        }

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
                //settings.username = textBox.Text;
            }
            Save();
        }
    }
}
