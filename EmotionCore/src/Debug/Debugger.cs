// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Engine;
using Emotion.Game.UI;
using Emotion.Game.UI.Layout;
using Emotion.Libraries;

#endregion

namespace Emotion.Debug
{
    /// <summary>
    /// Object which assists with debugging.
    /// </summary>
    public static class Debugger
    {
        /// <summary>
        /// Whether running in debug mode.
        /// </summary>
        public static bool DebugMode;

        #region Debug Assist Objects

        /// <summary>
        /// The corner anchor for debug UI components.
        /// </summary>
        public static CornerAnchor CornerAnchor { get; private set; }

        private static Controller _debugUIController;

        #endregion

        #region Declarations

        /// <summary>
        /// The next debug command to process.
        /// </summary>
        private static string _command;

        /// <summary>
        /// The thread the console is awaiting input on.
        /// </summary>
        private static Task _consoleThread;

        /// <summary>
        /// Highlighted source.
        /// </summary>
        private static MessageSource _highlighted = MessageSource.Other;

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the debugger and logging logic.
        /// </summary>
        [Conditional("DEBUG")]
        internal static void Initialize()
        {
            DebugMode = true;

            // Setup console command reading.
            _command = "";

            // Start input.
            StartConsoleInput();
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

            Context.ScriptingEngine.Expose("highlight", (Action<string>) (source =>
            {
                bool parsed = Enum.TryParse(source, true, out MessageSource parsedSource);
                if (!parsed) return;
                _highlighted = parsedSource;
            }), "Highlights the specified message source.");
        }

        [Conditional("DEBUG")]
        internal static void Stop()
        {
            _consoleThread.Dispose();
        }

        #endregion

        #region Console
        [Conditional("DEBUG")]
        private static void StartConsoleInput()
        {
            _consoleThread = new Task(ConsoleThread);
            _consoleThread.Start();
        }

        /// <summary>
        /// Processes console input without blocking the engine.
        /// </summary>
        private static void ConsoleThread()
        {
            // Check whether to expect console input.
            if (!Context.Flags.ConsoleInput) return;

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
            catch (Exception ex)
            {
                Context.Log.Error("Console thread has crashed.", ex, MessageSource.Debugger);
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
            Console.WriteLine(Context.ScriptingEngine.RunScript($"return {_command};"));
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