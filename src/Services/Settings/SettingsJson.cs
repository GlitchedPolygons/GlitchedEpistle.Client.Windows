/*
    Glitched Epistle - Windows Client
    Copyright (C) 2019 Raphael Beck

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Collections.Generic;

using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;

using Newtonsoft.Json;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings
{
    /// <summary>
    /// Implements the <see cref="ISettings" /> interface for accessing user settings formatted as human-readable JSON.
    /// </summary>
    /// <seealso cref="ISettings" />
    public class SettingsJson : ISettings
    {
        protected Dictionary<string, string> settings = new Dictionary<string, string>(16) { { "Version", App.Version } };

        /// <summary>
        /// Absolute settings file path.
        /// </summary>
        public string FilePath { get; }

        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsJson"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance needed for logging any eventual errors that might occur.</param>
        public SettingsJson(ILogger logger)
        {
            this.logger = logger;

            FilePath = Path.Combine(Paths.ROOT_DIRECTORY, "Settings.json");
        }

        /// <summary>
        /// Saves the current user settings out to disk.
        /// </summary>
        /// <returns>Whether the settings were saved out to disk successfully or not.</returns>
        public bool Save()
        {
            lock (settings)
            {
                settings["Version"] = App.Version;
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
        }

        /// <summary>
        /// Loads user settings from disk into the <see cref="ISettings" /> instance.
        /// </summary>
        /// <returns>Whether the loading procedure was successful or not.</returns>
        public bool Load()
        {
            lock (settings)
            {
                if (!File.Exists(FilePath))
                {
                    return false;
                }
                try
                {
                    settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(FilePath)) ?? new Dictionary<string, string>(16) { { "Version", App.Version } };
                    return true;
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message);
                    return false;
                }
            }
        }

        public string this[string key]
        {
            get => settings.TryGetValue(key, out string setting) ? setting : null;
            set
            {
                lock (settings)
                settings[key] = value;
            }
        }

        public string this[string key, string defaultValue] => settings.TryGetValue(key, out string setting) ? setting : defaultValue;

        public int this[string key, int defaultValue] => !settings.TryGetValue(key, out string settingString) || !int.TryParse(settingString, out int i) ? defaultValue : i;
        public bool this[string key, bool defaultValue] => !settings.TryGetValue(key, out string settingString) || !bool.TryParse(settingString, out bool i) ? defaultValue : i;
        public float this[string key, float defaultValue] => !settings.TryGetValue(key, out string settingString) || !float.TryParse(settingString, out float i) ? defaultValue : i;
        public double this[string key, double defaultValue] => !settings.TryGetValue(key, out string settingString) || !double.TryParse(settingString, out double i) ? defaultValue : i;
    }
}
