using System;
using System.IO;
using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Logging
{
    /// <summary>
    /// Logger <see langword="class"/> for logging messages, warnings and errors to the log files located inside the application's user directory.
    /// Implements the <see cref="GlitchedPolygons.GlitchedEpistle.Client.Services.Logging.ILogger" /> interface.
    /// </summary>
    /// <seealso cref="GlitchedPolygons.GlitchedEpistle.Client.Services.Logging.ILogger" />
    public class Logger : ILogger
    {
        /// <summary>
        /// Gets the directory path where the log files are stored on disk.
        /// </summary>
        /// <value>The directory path where the log files are stored on disk..</value>
        public string DirectoryPath { get; }

        /// <summary>
        /// Gets the message log file path.
        /// </summary>
        /// <value>The message log file path.</value>
        public string MessageLogFilePath { get; }

        /// <summary>
        /// Gets the warning log file path.
        /// </summary>
        /// <value>The warning log file path.</value>
        public string WarningLogFilePath { get; }

        /// <summary>
        /// Gets the error log file path.
        /// </summary>
        /// <value>The error log file path.</value>
        public string ErrorLogFilePath { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class (implements <see cref="ILogger"/>).
        /// </summary>
        public Logger()
        {
            DirectoryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GlitchedPolygons",
                "GlitchedEpistle",
                "Logs"
            );

            MessageLogFilePath = Path.Combine(
                DirectoryPath,
                "Messages.log"
            );

            WarningLogFilePath = Path.Combine(
                DirectoryPath,
                "Warnings.log"
            );

            ErrorLogFilePath = Path.Combine(
                DirectoryPath,
                "Errors.log"
            );
        }

        private void CheckDirectory()
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }
        }
        
        private string Timestamp(string msg) => $"[{DateTime.Now:s}] {msg}\n";

        /// <summary>
        /// Logs an innocent message.
        /// </summary>
        /// <param name="msg">The message.</param>
        public void LogMessage(string msg)
        {
            CheckDirectory();
            File.AppendAllText(MessageLogFilePath, Timestamp(msg));
        }

        /// <summary>
        /// Logs a warning.
        /// </summary>
        /// <param name="msg">The warning.</param>
        public void LogWarning(string msg)
        {
            CheckDirectory();
            File.AppendAllText(WarningLogFilePath, Timestamp(msg));
        }

        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="msg">The error.</param>
        public void LogError(string msg)
        {
            CheckDirectory();
            File.AppendAllText(ErrorLogFilePath, Timestamp(msg));
        }
    }
}
