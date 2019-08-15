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

using GlitchedPolygons.GlitchedEpistle.Client.Models;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Constants
{
    /// <summary>
    /// IO path constants (e.g. application root directory path).
    /// </summary>
    public static class Paths
    {
        /// <summary>
        /// The application's root directory where all the user settings, convos, etc... are stored.
        /// </summary>
        public static readonly string ROOT_DIRECTORY = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GlitchedPolygons",
            "GlitchedEpistle"
        );

        /// <summary>
        /// Get the convos storage directory for a specific user account's convos (creates the directory if it doesn't exist).
        /// </summary>
        /// <param name="userId"><see cref="User.Id"/></param>
        /// <returns>The path to the convos directory.</returns>
        public static string GetConvosDirectory(string userId) => Directory.CreateDirectory(Path.Combine(ROOT_DIRECTORY, userId, "Convos")).FullName;
    }
}
