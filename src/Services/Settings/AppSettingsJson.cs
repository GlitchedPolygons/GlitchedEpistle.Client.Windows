using System.IO;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings
{
    public class AppSettingsJson : SettingsJson, IAppSettings
    {
        public AppSettingsJson(ILogger logger) : base(logger)
        {
            FilePath = Path.Combine(Paths.ROOT_DIRECTORY, "Settings.json");
        }

        public string ClientVersion => settings["Version"];

        public string ServerUrl
        {
            get => settings["ServerUrl"];
            set => settings["ServerUrl"] = value;
        }

        public string LastUserId
        {
            get => settings["LastUserId"];
            set => settings["LastUserId"] = value;
        }
    }
}
