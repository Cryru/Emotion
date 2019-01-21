// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Debug.Logging.SoulLogging;
using Emotion.Engine;

#endregion

namespace Emotion.Debug.Logging
{
    /// <summary>
    /// The default Emotion logger. Uses a Soul.Logging ImmediateLoggerService and logs to the console if in debug.
    /// </summary>
    public sealed class DefaultLogger : LoggingProvider
    {
        private ImmediateLoggingService _logger;
        private Task _loggingThread;
        private ThreadManager _threadManager;
        private bool _running;

        /// <summary>
        /// Create a default logger.
        /// </summary>
        public DefaultLogger()
        {
            // Setup logging.
            _logger = new ImmediateLoggingService
            {
                LogLimit = 10,
                Limit = 2000,
                Stamp = "Emotion Engine Log"
            };

            // Create the thread manager.
            _threadManager = new ThreadManager("Logging Thread") {BlockOnExecution = false};

            // Start the logging thread.
            _running = true;
            _loggingThread = new Task(LoggingThread, TaskCreationOptions.LongRunning);
            _loggingThread.Start();
        }

        private void LoggingThread()
        {
            // Bind logging thread.
            _threadManager.BindThread();

            try
            {
                while (_running)
                {
                    // Run logging.
                    _threadManager.Run();
                }
            }
            catch (Exception ex)
            {
                if (Context.IsRunning && !(ex is ThreadAbortException)) Error("Logging thread has crashed.", new Exception(ex.Message, ex), MessageSource.Debugger);
            }
            finally
            {
                // Flush remaining.
                Dispose();
            }
        }

        /// <inheritdoc />
        public override void Log(MessageType type, MessageSource source, string message)
        {
            string threadStamp = $"[{Thread.CurrentThread.Name}/{Thread.CurrentThread.ManagedThreadId}]";
            _threadManager.ExecuteOnThread(() => { LogInternal(type, source, $"{threadStamp} {message}"); });
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            _running = false;

            while (!_threadManager.Empty)
            {
                _threadManager.Run();
            }
        }

        #region Helpers

        private void LogInternal(MessageType type, MessageSource source, string message)
        {
            // Change the color of the log depending on the type.
            switch (type)
            {
                case MessageType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case MessageType.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case MessageType.Trace:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case MessageType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }

            // Log and display the message.
            _logger.Log($"[{type}-{source}] {message}");
            if (type != MessageType.Trace) Console.WriteLine($"[{source}] {message}");

            // Restore the normal color.
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        #endregion
    }
}