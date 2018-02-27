// SoulEngine - https://github.com/Cryru/SoulEngine

#if DEBUG

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Soul.Engine.ECS;
using Soul.Engine.Enums;
using Soul.Engine.Modules;
using Soul.Engine.Scenography;
using Soul.Logging;

#endregion

namespace Soul.Engine.Diagnostics
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

        #endregion

        #region Module API

        /// <summary>
        /// Initializes the module.
        /// </summary>
        internal static void Setup()
        {
            // Start the boot timer.
            _bootTimer = new Stopwatch();
            _bootTimer.Start();

            // Send debugging boot messages.
            DebugMessage(DiagnosticMessageType.Core,
                "SoulEngine 2018 " + Assembly.GetExecutingAssembly().GetName().Version);
            DebugMessage(DiagnosticMessageType.Core, "Using: ");
            DebugMessage(DiagnosticMessageType.Core, " |- SoulLib: v" + Meta.Version);
            DebugMessage(DiagnosticMessageType.Core, " |- SoulPhysics: v" + Physics.Meta.Version);

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
                DebugMessage(DiagnosticMessageType.Core, "Boot took " + _bootTimer.ElapsedMilliseconds + " ms");
                _bootTimer.Stop();
                _bootTimer = null;
            }
        }

        #endregion

        #region Internal

        /// <summary>
        /// Processes console input without blocking the engine.
        /// </summary>
        private static void ConsoleThread()
        {
            while (Core.Context != null)
            {
                string readLine = Console.ReadLine();
                readLine = readLine?.Trim(' ');
                if (!string.IsNullOrEmpty(readLine)) Scripting.RunScript(readLine);
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Displays a debug message.
        /// </summary>
        /// <param name="type">The type of message.</param>
        /// <param name="message">The message itself.</param>
        internal static void DebugMessage(DiagnosticMessageType type, string message)
        {
            // Prevent logging from multiple threads messing up coloring and logging.
            lock (message)
            {
                // Change the color of the log depending on the type.
                switch (type)
                {
                    case DiagnosticMessageType.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case DiagnosticMessageType.Scripting:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    case DiagnosticMessageType.SceneManager:
                    case DiagnosticMessageType.Scene:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                        //case DiagnosticMessageType.InfoGreen:
                        //    Console.ForegroundColor = ConsoleColor.Green;
                        //    break;
                        //case DiagnosticMessageType.InfoDark:
                        //    Console.BackgroundColor = ConsoleColor.White;
                        //    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        //    break;
                        //case DiagnosticMessageType.Warning:
                        //    Console.ForegroundColor = ConsoleColor.Yellow;
                        //    break;
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
            Scripting.Expose("log", (Action<string>)(msg => DebugMessage(DiagnosticMessageType.Debugger, msg)));
            Scripting.Expose("help", (Action)Help);
            Scripting.Expose("stats", (Func<string>) ScriptLibrary.Statistics);
            Scripting.Expose("getEntities", (Func<string>) ScriptLibrary.GetEntities);
            Scripting.Expose("dump", (Action)Dump);
        }

        private static void Dump()
        {
            Scripting.RunScript(@"
              let registered = Object.keys(this);
          
              for (let i = 0; i < registered.length; i++) {
                  let message = registered[i];
          
                  try {
                      message += ' ' + this[registered[i]];
                  } catch (e) { }
          
                  log(message);
              }");
        }

        private static void Help()
        {
            Scripting.RunScript("log('SoulEngine 2018 Scripting Help Menu\nIN DEVELOPMENT')");
        }

        #endregion
    }
}

#endif