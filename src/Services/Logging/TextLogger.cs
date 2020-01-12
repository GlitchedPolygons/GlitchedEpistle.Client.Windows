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

using System;
using System.IO;

using GlitchedPolygons.GlitchedEpistle.Client.Services.Logging;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Services.Logging
{
    /// <summary>
    /// Logger <c>class</c> for logging messages, warnings and errors to the log files located inside the application's user directory.
    /// Implements the <see cref="GlitchedPolygons.GlitchedEpistle.Client.Services.Logging.ILogger" /> interface.
    /// </summary>
    /// <seealso cref="GlitchedPolygons.GlitchedEpistle.Client.Services.Logging.ILogger" />
    public class TextLogger : ILogger
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

        private object messageLock = new object();
        private object warningLock = new object();
        private object errorLock = new object();
        private object logLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="TextLogger"/> class (implements <see cref="ILogger"/>).
        /// </summary>
        public TextLogger()
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

        private static string Timestamp(string msg)
        {
            return $"[{DateTime.Now.ToString("s")}] {msg}\n";
        }

        /// <summary>
        /// Logs an innocent message.
        /// </summary>
        /// <param name="msg">The message.</param>
        public void LogMessage(string msg)
        {
            CheckDirectory();
            lock (messageLock)
            {
                Log(msg, MessageLogFilePath);
            }
        }

        /// <summary>
        /// Logs a warning.
        /// </summary>
        /// <param name="msg">The warning.</param>
        public void LogWarning(string msg)
        {
            CheckDirectory();
            lock (warningLock)
            {
                Log(msg, WarningLogFilePath);
            }
        }

        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="msg">The error.</param>
        public void LogError(string msg)
        {
            CheckDirectory();
            lock (errorLock)
            {
                Log(msg, ErrorLogFilePath);
            }
        }

        private void Log(string msg, string path)
        {
            try
            {
                lock (logLock)
                {
                    string log = string.Empty;
                    if (File.Exists(path))
                    {
                        log = File.ReadAllText(path);
                    }
                    File.WriteAllText(path, Timestamp(msg) + log);
                }
            }
            catch (Exception) { }
        }
    }
}
