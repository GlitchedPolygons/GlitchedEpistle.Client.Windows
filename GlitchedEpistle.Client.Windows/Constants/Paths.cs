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
        /// Get the convos storage directory for a specific user account.
        /// </summary>
        /// <param name="userId"><see cref="User.Id"/></param>
        /// <returns>The path to the convos directory.</returns>
        public static string GetConvosDirectory(string userId) => Path.Combine(ROOT_DIRECTORY, userId, "Convos");
    }
}
