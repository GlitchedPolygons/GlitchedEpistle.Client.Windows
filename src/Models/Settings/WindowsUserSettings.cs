using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GlitchedPolygons.GlitchedEpistle.Client.Models;
using GlitchedPolygons.GlitchedEpistle.Client.Models.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;

using Newtonsoft.Json;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Models.Settings
{
    public class WindowsUserSettings : UserSettings
    {
        public override string Username { get; set; }

        private readonly User user;
        private readonly ILogger logger;
        private readonly object settingsLock = new object();

        public WindowsUserSettings(User user, ILogger logger)
        {
            this.user = user;
            this.logger = logger;
        }

        public override bool Save()
        {
            lock (settingsLock)
            {
                try
                {
                    File.WriteAllText(Path.Combine(Paths.GetUserDirectory(user.Id), "Settings.json"), JsonConvert.SerializeObject(this, Formatting.Indented));
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
            string fp = Path.Combine(Paths.GetUserDirectory(user.Id), "Settings.json");
            lock (settingsLock)
            {
                if (!File.Exists(fp))
                {
                    return false;
                }
                try
                {
                    var settings = JsonConvert.DeserializeObject<WindowsUserSettings>(File.ReadAllText(fp));
                    Username = settings.Username;
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
