using System;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services
{
    /// <summary>
    /// Implements the <see cref="ISettings" /> interface for accessing user settings formatted as human-readable JSON.
    /// </summary>
    /// <seealso cref="ISettings" />
    public class SettingsJson : ISettings
    {
        private Dictionary<string, string> settings = new Dictionary<string, string>(16) { { "_version", App.VERSION } };

        public string DirectoryPath { get; }
        public string FilePath { get; }

        public SettingsJson()
        {
            DirectoryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GlitchedPolygons",
                "GlitchedEpistle"
            );

            FilePath = Path.Combine(
                DirectoryPath,
                "UserSettings.json"
            );
        }

        public bool Save()
        {
            settings["version"] = App.VERSION;
            try
            {
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
                return true;
            }
            catch (Exception e)
            {
                string error = $"[{DateTime.Now:O}] {e.Message}\n\n";
                // TODO: write this error to some errors.log file or something
                return false;
            }
        }

        public bool Load()
        {
            try
            {
                settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(FilePath));
                return true;
            }
            catch (Exception e)
            {
                string error = $"[{DateTime.Now:O}] {e.Message}\n\n";
                // TODO: write this error to some errors.log file or something
                return false;
            }
        }

        public string this[string key]
        {
            get => settings.TryGetValue(key, out string setting) ? setting : null;
            set => settings[key] = value;
        }

        public string this[string key, string defaultValue] => settings.TryGetValue(key, out string setting) ? setting : defaultValue;

        public int this[string key, int defaultValue] => !settings.TryGetValue(key, out string settingString) || !int.TryParse(settingString, out int i) ? defaultValue : i;
        public bool this[string key, bool defaultValue] => !settings.TryGetValue(key, out string settingString) || !bool.TryParse(settingString, out bool i) ? defaultValue : i;
        public float this[string key, float defaultValue] => !settings.TryGetValue(key, out string settingString) || !float.TryParse(settingString, out float i) ? defaultValue : i;
        public double this[string key, double defaultValue] => !settings.TryGetValue(key, out string settingString) || !double.TryParse(settingString, out double i) ? defaultValue : i;
    }
}
