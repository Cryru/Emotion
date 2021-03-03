#region Using

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Common;

#endregion

namespace Emotion.Standard.Logging
{
    /// <summary>
    /// The default desktop-platform based logger.
    /// </summary>
    public class DefaultLogger : LoggingProvider
    {
        private ConcurrentQueue<(MessageType, string)> _logQueue = new ConcurrentQueue<(MessageType, string)>();
        private AutoResetEvent _queueEvent = new AutoResetEvent(false);
        private bool _stdOut;
        private string _logFolder;
        private Task _logThread;
        private bool _logThreadRun = true;
        private const int MAX_LOG_FILES = 10;
        private const int MAX_LOG_SIZE = 10000000;

        /// <summary>
        /// Create a default logger.
        /// </summary>
        /// <param name="stdOut">Whether to redirect logs to STDOUT as well.</param>
        /// <param name="logFolder">The folder to log to. "Logs" by default/</param>
        public DefaultLogger(bool stdOut, string logFolder = "Logs")
        {
            _stdOut = stdOut;
            _logFolder = Path.Join(".", logFolder);
            try
            {
                // Keep only the last 10 logs. (retainedFileCountLimit doesn't work reliably)
                Directory.CreateDirectory(_logFolder);
                string[] fileCount = Directory.GetFiles(_logFolder);
                if (fileCount.Length > MAX_LOG_FILES)
                {
                    FileInfo[] filesWithDates = fileCount.Select(x => new FileInfo(x)).OrderBy(x => x.LastWriteTime.ToFileTime()).ToArray();

                    // Delete oldest.
                    try
                    {
                        for (var i = 0; i < filesWithDates.Length - MAX_LOG_FILES; i++)
                        {
                            filesWithDates[i].Delete();
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                _logThread = Task.Run(LogThread);
            }
            catch (Exception)
            {
                // ignored - no where to log it
            }
        }

        private string GenerateLogName()
        {
            return $".{Path.DirectorySeparatorChar}{_logFolder}{Path.DirectorySeparatorChar}{DateTime.Now:MM-dd-yyyy_HH-mm-ss}.log";
        }

        private void LogThread()
        {
            string fileName = GenerateLogName();
            string fileDirectory = Path.GetDirectoryName(fileName);
            if (fileDirectory != null) Directory.CreateDirectory(fileDirectory);

            TextWriter currentFileStream = File.CreateText(fileName);
            var fileSizeCounter = 0;
            TextWriter stdOut = _stdOut ? Console.Out : null;

            while (_logThreadRun || _logQueue.Count > 0)
            {
                if (Engine.Host?.NamedThreads ?? false) Thread.CurrentThread.Name ??= "Logging Thread";
                if (_logQueue.TryDequeue(out (MessageType type, string line) logItem))
                {
                    switch (logItem.type)
                    {
                        case MessageType.Error:
                            stdOut?.Write("\x1b[38;5;0045m[ERR]\x1b[38;5;0015m ");
                            currentFileStream?.Write("[ERR] ");
                            break;
                        case MessageType.Info:
                            stdOut?.Write("\x1b[38;5;0007m[INF]\x1b[38;5;0015m ");
                            currentFileStream?.Write("[INF] ");
                            break;
                        case MessageType.Trace:
                            stdOut?.Write("\x1b[38;5;0008m[DBG]\x1b[38;5;0015m ");
                            currentFileStream?.Write("[DBG] ");
                            break;
                        case MessageType.Warning:
                            stdOut?.Write("\x1b[38;5;0011m[WRN]\x1b[38;5;0015m ");
                            currentFileStream?.Write("[WRN] ");
                            break;
                        default:
                            stdOut?.WriteAsync("[???] ");
                            currentFileStream?.Write("[???] ");
                            break;
                    }

                    string line = logItem.line;
                    currentFileStream?.WriteLine(line);
                    currentFileStream?.Flush();
                    stdOut?.WriteLine(line);
                    fileSizeCounter += line.Length;

                    if (fileSizeCounter <= MAX_LOG_SIZE) continue;
                    fileName = GenerateLogName();
                    currentFileStream = File.CreateText(fileName);
                    fileSizeCounter = 0;
                }
                else
                {
                    _queueEvent.WaitOne();
                }
            }

            currentFileStream?.Flush();
        }

        /// <inheritdoc />
        public override void Log(MessageType type, string source, string message)
        {
            _logQueue.Enqueue((type, $"{Engine.TotalTime:0} [{source}] [{Thread.CurrentThread.Name}/{Thread.CurrentThread.ManagedThreadId:D2}] {message}"));
            _queueEvent.Set();
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            _logThreadRun = false;
            _queueEvent.Set();
            _logThread.Wait();
        }
    }
}