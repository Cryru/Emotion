// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Concurrent;
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
        private ConcurrentQueue<Tuple<MessageType, MessageSource, string>> _loggingQueue;
        private Thread _loggingThread;

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

            // Setup logging queue.
            _loggingQueue = new ConcurrentQueue<Tuple<MessageType, MessageSource, string>>();

            // Start the logging thread.
            _loggingThread = new Thread(LoggingThread) {Name = "Logging Thread"};
            _loggingThread.Start();
            while (!_loggingThread.IsAlive)
            {
            }
        }

        private void LoggingThread()
        {
            try
            {
                while (!Environment.HasShutdownStarted)
                {
                    // Check for logs every 100 ms.
                    Task.Delay(100).Wait();

                    // Perform logging.
                    while (!_loggingQueue.IsEmpty) LogThreadLoop();
                }

                FlushRemainingLogs();
            }
            catch (Exception ex)
            {
                if (Context.IsRunning && !(ex is ThreadAbortException)) Error("Logging thread has crashed.", new Exception(ex.Message, ex), MessageSource.Debugger);
            }
        }

        private void LogThreadLoop()
        {
            // Read from the logging queue.
            bool readLine = _loggingQueue.TryDequeue(out Tuple<MessageType, MessageSource, string> nextLog);
            if (!readLine) return;

            MessageType type = nextLog.Item1;
            MessageSource source = nextLog.Item2;
            string message = nextLog.Item3;

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

        private void FlushRemainingLogs()
        {
            // Flush logs.
            while (!_loggingQueue.IsEmpty) LogThreadLoop();
        }

        /// <inheritdoc />
        public override void Log(MessageType type, MessageSource source, string message)
        {
            _loggingQueue.Enqueue(new Tuple<MessageType, MessageSource, string>(type, source, $"[{Thread.CurrentThread.Name}/{Thread.CurrentThread.ManagedThreadId}] {message}"));
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            // Stop the logging thread.
            _loggingThread.Abort();
            while (_loggingThread.IsAlive)
            {
            }

            FlushRemainingLogs();
        }
    }
}