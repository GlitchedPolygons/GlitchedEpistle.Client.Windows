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
        /// The directory where the user keys will be stored.
        /// </summary>
        public static readonly string KEYS_DIRECTORY = Path.Combine(ROOT_DIRECTORY, "Keys");

        public static readonly string PUBLIC_KEY_PATH = Path.Combine(KEYS_DIRECTORY, "Public.rsa.pem");
        public static readonly string PRIVATE_KEY_PATH = Path.Combine(KEYS_DIRECTORY, "Private.rsa.pem");
    }
}
