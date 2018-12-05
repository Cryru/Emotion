// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace Soul.Logging
{
    /// <summary>
    /// An interface for a logging service.
    /// </summary>
    public abstract class LoggingService
    {
        #region Declarations

        /// <summary>
        /// The number of lines the log can have at the most.
        /// </summary>
        public int Limit { get; set; } = 1000;

        /// <summary>
        /// The number of logs that can be active at the most.
        /// </summary>
        public int LogLimit { get; set; } = 5;

        /// <summary>
        /// The log's stamp.
        /// </summary>
        public string Stamp { get; set; } = "";

        #endregion

        /// <summary>
        /// The current log.
        /// </summary>
        protected List<string> _currentLog;

        /// <summary>
        /// Initialization.
        /// </summary>
        protected LoggingService()
        {
            LogCheck();
            FolderCheck();
        }

        #region Functions

        /// <summary>
        /// Checks whether the log is initialized.
        /// </summary>
        protected virtual void LogCheck()
        {
            if (_currentLog == null) _currentLog = new List<string>();
        }

        /// <summary>
        /// Checks and creates logging folders.
        /// </summary>
        protected void FolderCheck()
        {
            Directory.CreateDirectory("Logs");
        }

        /// <summary>
        /// Adds a message to the log with a timestamp.
        /// </summary>
        /// <param name="message">The message to add.</param>
        protected virtual void AddToLog(string message)
        {
            // Format message.
            message = "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "/" + DateTime.Now.Millisecond + "] " + message;

            // Check if over limit when adding the next line.
            if (_currentLog.Count + 1 > Limit) LimitReached();

            _currentLog.Add(message);
        }

        /// <summary>
        /// Saves the log to a file.
        /// </summary>
        /// <param name="fileName">The filename to save under, if empty then a new name is generated.</param>
        protected void SaveLog(string fileName = "")
        {
            // Check if the log is empty.
            if (_currentLog.Count == 0) return;

            // Run the presave processing function.
            PresaveProcessing();

            // Compose filename.
            fileName = string.IsNullOrEmpty(fileName) ? "Logs" + Path.DirectorySeparatorChar + "Log_" + DateTime.Now.ToFileTime() + ".log" : fileName;

            // Check if the file exists, append a duplicate name tag, and if it still exists afterward (imagine that) delete it.
            if (File.Exists(fileName)) fileName += "_" + "DuplicateName-" + DateTime.Now.Ticks;
            if (File.Exists(fileName)) TryDeleteLog(fileName);

            // Check if log limit reached.
            string[] files = Directory.GetFiles("Logs");
            if (files.Length >= LogLimit) TryDeleteLog(files[0]);

            // Write to a file.
            File.WriteAllLines(fileName, _currentLog.ToArray());
        }

        #endregion

        #region Abstract

        /// <summary>
        /// The function to call when the limit is reached.
        /// </summary>
        protected virtual void LimitReached()
        {
        }

        /// <summary>
        /// Log processing before it is saved.
        /// </summary>
        protected virtual void PresaveProcessing()
        {
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Tries to delete a log.
        /// </summary>
        protected static void TryDeleteLog(string logName)
        {
            File.Delete(logName);
        }

        #endregion
    }
}