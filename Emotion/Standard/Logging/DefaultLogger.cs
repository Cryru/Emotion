#region Using

using System;
using System.IO;
using System.Linq;
using System.Threading;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

#endregion

namespace Emotion.Standard.Logging
{
    /// <summary>
    /// The default logger. Uses SeriLog.
    /// </summary>
    public class DefaultLogger : LoggingProvider
    {
        /// <summary>
        /// SeriLog logger instance.
        /// </summary>
        private Logger _logger;

        /// <summary>
        /// Create a default logger.
        /// </summary>
        /// <param name="stdOut">Whether to redirect logs to STDOUT as well.</param>
        /// <param name="logFolder">The folder to log to. "Logs" by default/</param>
        public DefaultLogger(bool stdOut, string logFolder = "Logs")
        {
            string fileName = $".{Path.DirectorySeparatorChar}{logFolder}{Path.DirectorySeparatorChar}{DateTime.Now:MM-dd-yyyy_HH-mm-ss}.log";

            LoggerConfiguration loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Verbose)
                // Limit to 10 files, and maximum of 100mb per file.
                .WriteTo.Async(a => a.File(fileName, fileSizeLimitBytes: 10000000, retainedFileCountLimit: 10));

            // Keep only the last 10 logs.
            string[] fileCount = Directory.GetFiles(logFolder);
            if (fileCount.Length > 10)
            {
                FileInfo[] filesWithDates = fileCount.Select(x => new FileInfo(x)).OrderBy(x => x.LastWriteTime.ToFileTime()).ToArray();

                // Delete oldest, until number is back to 10.
                try
                {
                    for (var i = 0; i < filesWithDates.Length - 10; i++)
                    {
                        filesWithDates[i].Delete();
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            if (stdOut) loggerConfig.WriteTo.Console(LogEventLevel.Verbose, theme: AnsiConsoleTheme.Code, outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}");

            _logger = loggerConfig.CreateLogger();
        }

        /// <inheritdoc />
        public override void Log(MessageType type, string source, string message)
        {
            string fullMessage = $"[{source}] [{Thread.CurrentThread.Name}/{Thread.CurrentThread.ManagedThreadId}] {message}";

            switch (type)
            {
                case MessageType.Error:
                    _logger.Error(fullMessage);
                    break;
                case MessageType.Info:
                    _logger.Information(fullMessage);
                    break;
                case MessageType.Trace:
                    _logger.Debug(fullMessage);
                    break;
                case MessageType.Warning:
                    _logger.Warning(fullMessage);
                    break;
                default:
                    _logger.Verbose(fullMessage);
                    break;
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            _logger.Dispose();
        }
    }
}