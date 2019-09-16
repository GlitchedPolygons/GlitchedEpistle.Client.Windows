using System;
using System.IO;
using System.Windows;

using GlitchedPolygons.GlitchedEpistle.Client.Models.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;

using Newtonsoft.Json;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Models.Settings
{
    /// <summary>
    /// Windows-client application settings.
    /// </summary>
    public class WindowsAppSettings : AppSettings
    {
        /// <summary>
        /// The Windows client theme.
        /// </summary>
        public enum WindowsEpistleTheme
        {
            DarkPolygons = 0,
            LightPolygons = 1,
            OLED = 2
        }

        public override string Version => App.Version;
        public override string ServerUrl { get; set; } 
        public override string LastUserId { get; set; }

        public WindowsEpistleTheme Theme { get; set; } = 0;

        public WindowState WindowState { get; set; } = WindowState.Normal;

        public int MainWindowWidth { get; set; } = 900;
        public int MainWindowHeight { get; set; } = 600;
        public int SidebarWidth { get; set; } = 345;

        [JsonIgnore]
        private string FilePath => Path.Combine(Paths.ROOT_DIRECTORY, "Settings.json");

        [JsonIgnore]
        private readonly ILogger logger;

        [JsonIgnore]
        private readonly object settingsLock = new object();

        public WindowsAppSettings(ILogger logger)
        {
            this.logger = logger;
        }

        public override bool Save()
        {
            lock (settingsLock)
            {
                try
                {
                    File.WriteAllText(FilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
                    return true;
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message);
                    return false;
                }
            }
        }

        public override bool Load()
        {
            lock (settingsLock)
            {
                if (!File.Exists(FilePath))
                {
                    return false;
                }
                try
                {
                    var settings = JsonConvert.DeserializeObject<WindowsAppSettings>(File.ReadAllText(FilePath));
                    ServerUrl = settings.ServerUrl;
                    LastUserId = settings.LastUserId;
                    Theme = settings.Theme;
                    return true;
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message);
                    return false;
                }
            }
        }
    }
}
