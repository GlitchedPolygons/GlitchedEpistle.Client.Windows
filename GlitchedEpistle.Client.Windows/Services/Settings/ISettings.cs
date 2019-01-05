namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings
{
    /// <summary>
    /// Service interface for accessing, saving and loading user settings.
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Saves the current user settings out to disk.
        /// </summary>
        /// <returns>Whether the settings were saved out to disk successfully or not.</returns>
        bool Save();

        /// <summary>
        /// Loads user settings from disk into the <see cref="ISettings"/> instance.
        /// </summary>
        /// <returns>Whether the loading procedure was successful or not.</returns>
        bool Load();

        /// <summary>
        /// Gets or sets a user setting with its specified key <see langword="string"/>.<para> </para>
        /// If you are trying to get an inexistent setting, <see langword="null"/> (or <c>string.Empty</c>) should be returned.<para> </para>
        /// If you are trying to set an inexistent setting, the setting shall be created.
        /// </summary>
        /// <param name="key">The setting's name/key.</param>
        /// <returns>The setting's <c>string</c> value; <see langword="null"/> (or <c>string.Empty</c>) if the setting doesn't exist.</returns>
        string this[string key] { get; set; }

        /// <summary>
        /// Gets a user setting by its key <c>string</c>.
        /// </summary>
        /// <param name="key">The setting's name/key.</param>
        /// <param name="defaultValue">The setting's default <c>string</c> value (in case the setting doesn't exist).</param>
        /// <returns>The setting's <c>string</c> value; the specified default value if the setting wasn't found.</returns>
        string this[string key, string defaultValue] { get; }

        /// <summary>
        /// Gets a user setting parsed as an <c>int</c>.
        /// </summary>
        /// <param name="key">The setting's key.</param>
        /// <param name="defaultValue">The setting's default <c>int</c> value to return in case the setting doesn't exist or couldn't be parsed.</param>
        /// <returns>The setting's <c>int</c> value; or the specified default value if the setting wasn't found or couldn't be parsed.</returns>
        int this[string key, int defaultValue] { get;  }

        /// <summary>
        /// Gets a user setting parsed as a <c>bool</c>.
        /// </summary>
        /// <param name="key">The setting's key.</param>
        /// <param name="defaultValue">The setting's default <c>bool</c> value to return in case the setting doesn't exist or couldn't be parsed.</param>
        /// <returns>The setting's <c>bool</c> value; or the specified default value if the setting wasn't found or couldn't be parsed.</returns>
        bool this[string key, bool defaultValue] { get;  }

        /// <summary>
        /// Gets a user setting parsed as a <c>float</c>.
        /// </summary>
        /// <param name="key">The setting's key.</param>
        /// <param name="defaultValue">The setting's default <c>float</c> value to return in case the setting doesn't exist or couldn't be parsed.</param>
        /// <returns>The setting's <c>float</c> value; or the specified default value if the setting wasn't found or couldn't be parsed.</returns>
        float this[string key, float defaultValue] { get;  }

        /// <summary>
        /// Gets a user setting parsed as a <c>double</c>.
        /// </summary>
        /// <param name="key">The setting's key.</param>
        /// <param name="defaultValue">The setting's default <c>double</c> value to return in case the setting doesn't exist or couldn't be parsed.</param>
        /// <returns>The setting's <c>double</c> value; or the specified default value if the setting wasn't found or couldn't be parsed.</returns>
        double this[string key, double defaultValue] { get;  }
    }
}
