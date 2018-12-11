using System;
using System.IO;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Logging
{
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

        public void LogMessage(string msg)
        {
            CheckDirectory();
            File.AppendAllText(MessageLogFilePath, Timestamp(msg));
        }

        public void LogWarning(string msg)
        {
            CheckDirectory();
            File.AppendAllText(WarningLogFilePath, Timestamp(msg));
        }

        public void LogError(string msg)
        {
            CheckDirectory();
            File.AppendAllText(ErrorLogFilePath, Timestamp(msg));
        }
    }
}
