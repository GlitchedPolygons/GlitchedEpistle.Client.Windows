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

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;

using Newtonsoft.Json;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings
{
    /// <summary>
    /// Implements the <see cref="ISettings" /> interface for accessing settings formatted as human-readable JSON.
    /// </summary>
    /// <seealso cref="ISettings" />
    public abstract class SettingsJson : ISettings
    {
        /// <summary>
        /// Internal settings dictionary.
        /// </summary>
        private IDictionary<string, string> settings = new Dictionary<string, string>(16) { { "Version", App.Version } };

        /// <summary>
        /// Absolute settings file path.
        /// </summary>
        public string FilePath { get; protected set; }

        /// <summary>
        /// <see cref="ILogger"/> for logging errors, messages and warnings.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsJson"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance needed for logging any eventual errors that might occur.</param>
        /// <param name="filePath">The json file's path.</param>
        protected SettingsJson(ILogger logger, string filePath)
        {
            this.logger = logger;
            FilePath = filePath;

            Reload();
        }

        protected void Reload()
        {
            if (FilePath.NullOrEmpty())
            {
                return;
            }

            lock (settings)
            {
                if (File.Exists(FilePath))
                {
                    try
                    {
                        settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(FilePath)) ?? new Dictionary<string, string>(16) { { "Version", App.Version } };
                    }
                    catch (Exception e)
                    {
                        logger?.LogError($"Failed to load config json file: {e.ToString()} - file path: {FilePath}");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a user setting with its specified key <c>string</c>.<para> </para>
        /// Setting should also auto-save the config.<para> </para>
        /// If you are trying to get an inexistent setting, <c>null</c> (or <c>string.Empty</c>) should be returned.<para> </para>
        /// If you are trying to set an inexistent setting, the setting shall be created.
        /// </summary>
        /// <param name="key">The setting's name/key.</param>
        /// <returns>The setting's <c>string</c> value; <c>null</c> (or <c>string.Empty</c>) if the setting doesn't exist.</returns>
        public string this[string key]
        {
            get => settings.TryGetValue(key, out string setting) ? setting : null;
            set
            {
                lock (settings)
                {
                    settings[key] = value;
                    settings["Version"] = App.Version;
                    try
                    {
                        File.WriteAllText(FilePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
                    }
                    catch (Exception e)
                    {
                        logger?.LogError($"Failed to save settings out to json file: {e.ToString()}");
                    }
                }
            }
        }

        /// <summary>
        /// Gets a user setting by its key <c>string</c>.
        /// </summary>
        /// <param name="key">The setting's name/key.</param>
        /// <param name="defaultValue">The setting's default <c>string</c> value (in case the setting doesn't exist).</param>
        /// <returns>The setting's <c>string</c> value; the specified default value if the setting wasn't found.</returns>
        public string this[string key, string defaultValue] => settings.TryGetValue(key, out string setting) ? setting : defaultValue;
        /// <summary>
        /// Gets a user setting parsed as an <c>int</c>.
        /// </summary>
        /// <param name="key">The setting's key.</param>
        /// <param name="defaultValue">The setting's default <c>int</c> value to return in case the setting doesn't exist or couldn't be parsed.</param>
        /// <returns>The setting's <c>int</c> value; or the specified default value if the setting wasn't found or couldn't be parsed.</returns>
        public int this[string key, int defaultValue] => !settings.TryGetValue(key, out string settingString) || !int.TryParse(settingString, out int i) ? defaultValue : i;
        /// <summary>
        /// Gets a user setting parsed as a <c>bool</c>.
        /// </summary>
        /// <param name="key">The setting's key.</param>
        /// <param name="defaultValue">The setting's default <c>bool</c> value to return in case the setting doesn't exist or couldn't be parsed.</param>
        /// <returns>The setting's <c>bool</c> value; or the specified default value if the setting wasn't found or couldn't be parsed.</returns>
        public bool this[string key, bool defaultValue] => !settings.TryGetValue(key, out string settingString) || !bool.TryParse(settingString, out bool i) ? defaultValue : i;
        /// <summary>
        /// Gets a user setting parsed as a <c>float</c>.
        /// </summary>
        /// <param name="key">The setting's key.</param>
        /// <param name="defaultValue">The setting's default <c>float</c> value to return in case the setting doesn't exist or couldn't be parsed.</param>
        /// <returns>The setting's <c>float</c> value; or the specified default value if the setting wasn't found or couldn't be parsed.</returns>
        public float this[string key, float defaultValue] => !settings.TryGetValue(key, out string settingString) || !float.TryParse(settingString, out float i) ? defaultValue : i;
        /// <summary>
        /// Gets a user setting parsed as a <c>double</c>.
        /// </summary>
        /// <param name="key">The setting's key.</param>
        /// <param name="defaultValue">The setting's default <c>double</c> value to return in case the setting doesn't exist or couldn't be parsed.</param>
        /// <returns>The setting's <c>double</c> value; or the specified default value if the setting wasn't found or couldn't be parsed.</returns>
        public double this[string key, double defaultValue] => !settings.TryGetValue(key, out string settingString) || !double.TryParse(settingString, out double i) ? defaultValue : i;
    }
}
