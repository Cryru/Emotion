// SoulEngine - https://github.com/Cryru/SoulEngine

#if DEBUG

#region Using

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Soul.Engine.Enums;
using Soul.Logging;

#endregion

namespace Soul.Engine.Modules
{
    /// <summary>
    /// The module used in debug mode to debug the engine.
    /// </summary>
    internal static class Debugging
    {
        #region Declarations

        #region Boot Timer

        /// <summary>
        /// Whether this is the first update for the module.
        /// </summary>
        private static bool _firstUpdate = true;

        /// <summary>
        /// Timer for the boot time.
        /// </summary>
        private static Stopwatch _bootTimer;

        /// <summary>
        /// The logging service.
        /// </summary>
        public static ImmediateLoggingService Logger = new ImmediateLoggingService
        {
            LogLimit = 10,
            Limit = 2000,
            Stamp = "==========\n" + "Log " + DateTime.Now + "\n=========="
        };

        #endregion

        /// <summary>
        /// The next debug command to process.
        /// </summary>
        private static string _command = "";

        /// <summary>
        /// The process handle of the application.
        /// </summary>
        private static Process _currentProcess;

        #endregion

        #region Module API

        /// <summary>
        /// Initializes the module.
        /// </summary>
        internal static void Setup()
        {
            // Get the process.
            _currentProcess = Process.GetCurrentProcess();

            // Start the boot timer.
            _bootTimer = new Stopwatch();
            _bootTimer.Start();

            // Send debugging boot messages.
            DebugMessage(DebugMessageType.Info,
                "SoulEngine 2018 " + Assembly.GetExecutingAssembly().GetName().Version);
            DebugMessage(DebugMessageType.Info, "Using: ");
            DebugMessage(DebugMessageType.Info, " |- Breath: v" + Breath.Meta.Version);
            DebugMessage(DebugMessageType.Info, " |- SoulLib: v" + Soul.Library.Meta.Version);
            DebugMessage(DebugMessageType.Info, " |- SoulPhysics: v" + Physics.Meta.Version);

            // Setup the debugging library.
            SetupDebuggingLibrary();

            // Start the console thread.
            Thread consoleThread = new Thread(ConsoleThread);
            consoleThread.Start();
        }

        /// <summary>
        /// Updates the module.
        /// </summary>
        internal static void Update()
        {
            // Check if this is the first update cycle.
            if (_firstUpdate)
            {
                _firstUpdate = false;
                // Log boot time and dispose of the stopwatch.
                DebugMessage(DebugMessageType.Info, "Boot took " + _bootTimer.ElapsedMilliseconds + " ms");
                _bootTimer.Stop();
                _bootTimer = null;
            }

            // Check if there is a command to execute.
            if (_command != string.Empty)
            {
                Scripting.RunScript(_command);
                _command = "";
            }
        }

        #endregion

        #region Internal

        /// <summary>
        /// Processes console input without blocking the engine.
        /// </summary>
        private static void ConsoleThread()
        {
            while (Core.Running)
            {
                string readLine = Console.ReadLine();
                if (readLine != null) _command = readLine.Trim(' ');
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Displays a debug message.
        /// </summary>
        /// <param name="type">The type of message.</param>
        /// <param name="message">The message itself.</param>
        internal static void DebugMessage(DebugMessageType type, string message)
        {
            // Prevent logging from multiple threads messing up coloring and logging.
            lock (message)
            {
                // Change the color of the log depending on the type.
                switch (type)
                {
                    case DebugMessageType.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case DebugMessageType.Info:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    case DebugMessageType.InfoBlue:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case DebugMessageType.InfoGreen:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case DebugMessageType.InfoDark:
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        break;
                    case DebugMessageType.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                }

                Logger.Log("[" + type + "] " + message);
                Console.WriteLine(message);

                // Restore the normal color.
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
            }
        }

        #endregion

        #region Debugging Script Library

        /// <summary>
        /// Adds debugging functions to the scripting engine.
        /// </summary>
        private static void SetupDebuggingLibrary()
        {
            Scripting.Expose("log", (Action) Console.WriteLine);
            Scripting.Expose("help", (Action)Help);
            Scripting.Expose("stats", (Func<string>)Statistics);
            Scripting.Expose("dump", (Action) Dump);
        }

        private static string Statistics()
        {
            return "Ram Usage: " + (_currentProcess.PrivateMemorySize64 / 1024 / 1024) + "mb\n" + 
                "Current Scene: " + SceneManager.CurrentScene.ToString() + "\n" +
                "Entities: " + SceneManager.CurrentScene?.RegisteredEntities.Count + "\n" + 
                "Running Systems: " + SceneManager.CurrentScene?.RunningSystems.Count;
        }

        private static void Dump()
        {
            Scripting.RunScript("" +
                                "let registered = Object.keys(this);" +
                                "log(registered);" +
                                "for(let i = 0; i < registered.length; i++) {" +
                                "   log(registered[i] + ' ' + this[registered[i]]);" +
                                "}" +
                                "");
        }

        private static void Help()
        {
            Scripting.RunScript("Object.keys(this)");
        }
        #endregion
    }
}

#endif