// Soul - https://github.com/Cryru/Soul

#region Using

using System;
using System.IO;
using System.Text;
using Soul.Logging;

#endregion

namespace Emotion.Debug.Logging.SoulLogging
{
    /// <inheritdoc />
    /// <summary>
    /// An immediate logging service. When a message is logged it is instantly written to the disk.
    /// </summary>
    public class ImmediateLoggingService : LoggingService
    {
        #region Internal Variables

        /// <summary>
        /// The writing stream of the log currently in use.
        /// </summary>
        private FileStream _logStream;

        /// <summary>
        /// The current line in the log. Used to keep track of limits.
        /// </summary>
        private int _line;

        #endregion

        /// <summary>
        /// Adds a message to the log.
        /// </summary>
        /// <param name="message">The message to add.</param>
        public void Log(string message)
        {
            // Safety checks.
            FolderCheck();
            LogCheck();

            // Add the message to the log.
            AddToLog(message);
        }

        /// <summary>
        /// If forcing a dump, make a new log.
        /// </summary>
        public void ForceDump()
        {
            // Delete the current log.
            _logStream.Flush();
            _logStream.Close();
            _logStream.Dispose();
            _logStream = null;
        }

        #region Internals

        /// <summary>
        /// Adds a message to the log with a timestamp.
        /// </summary>
        /// <param name="message">The message to add.</param>
        protected override void AddToLog(string message)
        {
            // Format message.
            message = "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "/" + DateTime.Now.Millisecond + "] " + message;

            // Check if over limit when adding the next line.
            if (_line + 1 > Limit)
                LimitReached();

            // Write the message.
            WriteToLog(message);
        }

        /// <summary>
        /// Writes a message to the log stream.
        /// </summary>
        /// <param name="message"></param>
        private void WriteToLog(string message)
        {
            if (_line != 0)
            {
                message = "\n" + message;
            }
            else
            {
                if (!string.IsNullOrEmpty(Stamp))
                    message = Stamp + "\n" + message;
            }

            byte[] byteMessage = Encoding.UTF8.GetBytes(message);
            _logStream.Write(byteMessage, 0, byteMessage.Length);

            // Flush the file.
            _logStream.Flush();

            // Increment the tracker.
            _line++;
        }

        /// <summary>
        /// Checks if a logging stream has been initialized.
        /// </summary>
        protected override void LogCheck()
        {
            // Safety check.
            FolderCheck();

            // Check if a stream has been initialized.
            if (_logStream != null) return;

            // If not, initialize one.
            string fileName = "Logs" + Path.DirectorySeparatorChar + "Log_" + DateTime.Now.ToFileTime() +
                              ".log";

            // Check if the file exists.
            if (File.Exists(fileName)) TryDeleteLog(fileName);

            // Check if log limit reached.
            string[] files = Directory.GetFiles("Logs");
            if (files.Length >= LogLimit) TryDeleteLog(files[0]);

            // Create the new log.
            _logStream = File.Create(fileName);

            // Reset tracker.
            _line = 0;

            // Check if there is a stamp.
            if (!string.IsNullOrEmpty(Stamp))
                WriteToLog(Stamp);
        }

        /// <summary>
        /// When the limit is reached, create a new one.
        /// </summary>
        protected override void LimitReached()
        {
            // Delete the current log.
            ForceDump();

            // Create a new one.
            LogCheck();
        }

        #endregion
    }
}