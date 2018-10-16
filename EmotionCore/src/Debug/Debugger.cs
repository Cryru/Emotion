// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Game.UI;
using Emotion.Game.UI.Layout;
using Emotion.System;
using Emotion.Utils;
using Soul.Logging;

#endregion

namespace Emotion.Debug
{
    public static class Debugger
    {
        #region Debug Assist Objects

        public static CornerAnchor CornerAnchor { get; private set; }
        private static Controller _debugUIController;

        #endregion

        #region Declarations

        /// <summary>
        /// A Soul.Logging service which logs all debug messages to a file.
        /// </summary>
        private static ImmediateLoggingService _logger;

        /// <summary>
        /// The next debug command to process.
        /// </summary>
        private static string _command;

        /// <summary>
        /// The number of messages queued to be logged.
        /// </summary>
        private static ConcurrentQueue<Tuple<MessageType, MessageSource, string>> _loggingQueue;

        /// <summary>
        /// The thread logging is done on.
        /// </summary>
        private static Thread _loggingThread;

        /// <summary>
        /// The thread the console is awaiting input on.
        /// </summary>
        private static Thread _consoleThread;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the debugger and logging logic.
        /// </summary>
        [Conditional("DEBUG")]
        internal static void Initialize()
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
            _loggingThread = new Thread(() => LoggingThread()) {Name = "Logging Thread"};
            _loggingThread.Start();
            while (!_loggingThread.IsAlive)
            {
            }

            // Setup console command reading.
            _command = "";

            // Start the console thread.
            _consoleThread = new Thread(() => ConsoleThread()) {Name = "Console Thread"};
            _consoleThread.Start();
            while (!_consoleThread.IsAlive)
            {
            }
        }

        /// <summary>
        /// Initializes the debugger as a module. This is done after the bootstrapping process is complete.
        /// </summary>
        [Conditional("DEBUG")]
        internal static void InitializeModule()
        {
            _debugUIController = new Controller();
            CornerAnchor = new CornerAnchor();
            _debugUIController.Add(CornerAnchor);
        }

        [Conditional("DEBUG")]
        internal static void Stop()
        {
            _consoleThread.Abort();
            while (_consoleThread.IsAlive)
            {
            }

            _loggingThread.Abort();
            while (_loggingThread.IsAlive)
            {
            }

            // Log anything left.
            while (LogInProgress()) LogThreadLoop();
        }

        #endregion

        #region Logging

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="type">The type of message to log.</param>
        /// <param name="source">The source of the message.</param>
        /// <param name="message">The message itself.</param>
        [Conditional("DEBUG")]
        public static void Log(MessageType type, MessageSource source, string message)
        {
            _loggingQueue.Enqueue(new Tuple<MessageType, MessageSource, string>(type, source, $"[{Thread.CurrentThread.Name}/{Thread.CurrentThread.ManagedThreadId}] {message}"));
        }

        /// <summary>
        /// Processes console input without blocking the engine.
        /// </summary>
        [Conditional("DEBUG")]
        private static void LoggingThread()
        {
            try
            {
                while (!Environment.HasShutdownStarted)
                {
                    // Sleep.
                    Task.Delay(1).Wait();

                    // Perform loop.
                    LogThreadLoop();
                }
            }
            catch (Exception)
            {
                // Where is this going to be logged lul.
                Log(MessageType.Error, MessageSource.Debugger, "Logging thread has crashed.");
            }
        }

        private static void LogThreadLoop()
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
            _logger.Log("[" + type + "-" + source + "] " + message);
            if (type != MessageType.Trace) Console.WriteLine("[" + source + "] " + message);

            // Restore the normal color.
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        /// <summary>
        /// Whether something is in the logging pipeline.
        /// </summary>
        /// <returns></returns>
        public static bool LogInProgress()
        {
            return !_loggingQueue.IsEmpty;
        }

        #endregion

        #region Console

        /// <summary>
        /// Processes console input without blocking the engine.
        /// </summary>
        [Conditional("DEBUG")]
        private static void ConsoleThread()
        {
            // Delay console thread on other OS as crashes happen.
            if (CurrentPlatform.OS != PlatformName.Windows) Thread.Sleep(2000);

            try
            {
                while (!Environment.HasShutdownStarted)
                {
                    string readLine = Console.ReadLine();
                    if (readLine != null) _command = readLine.Trim(' ');
                }
            }
            catch (Exception)
            {
                Log(MessageType.Error, MessageSource.Debugger, "Console thread has crashed.");
            }
        }

        #endregion

        #region Loops

        /// <summary>
        /// Is run every tick by the platform context.
        /// </summary>
        [Conditional("DEBUG")]
        internal static void Update()
        {
            // Check if there is a command to execute.
            if (_command == string.Empty) return;
            Context.ScriptingEngine.RunScript(_command);
            _command = "";

            // Update the debug UI controller.
            _debugUIController.Update();
        }

        /// <summary>
        /// Is run every tick by the platform context.
        /// </summary>
        [Conditional("DEBUG")]
        internal static void Draw()
        {
            _debugUIController.Draw();
        }

        #endregion
    }
}