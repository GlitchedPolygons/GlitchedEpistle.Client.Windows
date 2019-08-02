using System;
using System.IO;

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
        /// The directory path where convos will be stored.
        /// </summary>
        public static readonly string CONVOS_DIRECTORY = Path.Combine(ROOT_DIRECTORY, "Convos");
    }
}
