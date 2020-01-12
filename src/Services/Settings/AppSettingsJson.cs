/*
    Glitched Epistle - Windows Client
    Copyright (C) 2020 Raphael Beck

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

using System.IO;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Settings;
using GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Settings
{
    /// <summary>
    /// Application-level settings that persist between user accounts.
    /// </summary>
    /// <seealso cref="ISettings"/>
    public class AppSettingsJson : SettingsJson, IAppSettings
    {
        public AppSettingsJson(ILogger logger) : base(logger, Path.Combine(Paths.ROOT_DIRECTORY, "Settings.json"))
        {
        }

        /// <summary>The Epistle client version.</summary>
        public string ClientVersion => this["Version", App.Version];

        /// <summary>The Epistle server URL to connect to.</summary>
        public string ServerUrl
        {
            get => this["ServerUrl"];
            set => this["ServerUrl"] = value;
        }

        /// <summary>The last used user id.</summary>
        public string LastUserId
        {
            get => this["LastUserId"];
            set => this["LastUserId"] = value;
        }
    }
}
