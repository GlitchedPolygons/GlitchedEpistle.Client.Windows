using System.IO;

using GlitchedPolygons.ExtensionMethods;
using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings
{
    public class UserSettingsJson : SettingsJson, IUserSettings
    {
        private readonly User user;
        private readonly ILogger logger;

        public UserSettingsJson(ILogger logger, User user) : base(logger)
        {
            this.user = user;
            this.logger = logger;

            if (!settings.ContainsKey("Username"))
            {
                settings["Username"] = user?.Id ?? "user";
            }
        }

        public string Username
        {
            get => settings["Username"];
            set => settings["Username"] = value;
        }

        public override bool Load()
        {
            if (user.Id.NullOrEmpty())
            {
                return false;
            }
            FilePath = Path.Combine(Paths.ROOT_DIRECTORY, user.Id, "Settings.json");
            return base.Load();
        }
    }
}
