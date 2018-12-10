using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using Newtonsoft.Json;
using Path = System.IO.Path;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        /// <summary>
        /// Absolute settings directory path.
        /// </summary>
        public string DirectoryPath { get; } 

        /// <summary>
        /// Absolute settings file path.
        /// </summary>
        public string FilePath { get; }
        
        public SettingsWindow()
        {
            DirectoryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GlitchedPolygons",
                "GlitchedEpistle"
            );

            FilePath = Path.Combine(
                DirectoryPath,
                "Settings.json"
            );

            InitializeComponent();

            Load();
            Closed += Settings_Closed;
        }

        private void Settings_Closed(object sender, EventArgs e)
        {
            Save();
            Closed -= Settings_Closed;
        }

        private void CheckDirectory()
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }
        }

        private void Save()
        {
            CheckDirectory();
            //File.WriteAllText(FilePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        private void Load()
        {
            CheckDirectory();

            if (!File.Exists(FilePath))
            {
                return;
            }

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
