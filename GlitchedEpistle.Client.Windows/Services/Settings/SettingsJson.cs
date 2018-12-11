using System;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Logging;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings
{
    /// <summary>
    /// Implements the <see cref="ISettings" /> interface for accessing user settings formatted as human-readable JSON.
    /// </summary>
    /// <seealso cref="ISettings" />
    public class SettingsJson : ISettings
    {
        private Dictionary<string, string> settings = new Dictionary<string, string>(16) { { "version", App.VERSION } };

        /// <summary>
        /// Absolute settings directory path.
        /// </summary>
        public string DirectoryPath { get; }

        /// <summary>
        /// Absolute settings file path.
        /// </summary>
        public string FilePath { get; }

        private readonly ILogger logger;

        public SettingsJson(ILogger logger)
        {
            this.logger = logger;

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
                logger.LogError(e.Message);
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
                logger.LogError(e.Message);
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
