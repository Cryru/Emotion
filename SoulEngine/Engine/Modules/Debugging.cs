// SoulEngine - https://github.com/Cryru/SoulEngine

#if DEBUG

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            DebugMessage(DebugMessageType.Info, " |- SoulLib: v" + Library.Meta.Version);
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
            Scripting.Expose("log", (Action<string>)(msg => DebugMessage(DebugMessageType.InfoBlue, msg)));
            Scripting.Expose("help", (Action)Help);
            Scripting.Expose("stats", (Action)Statistics);
            Scripting.Expose("dump", (Action)Dump);
        }

        private static void Statistics()
        {
            List<string> data = new List<string>();

            data.Add("Ram Usage: " + _currentProcess.PrivateMemorySize64 / 1024 / 1024 + "mb");
            data.Add("Current Scene: " + SceneManager.CurrentScene);

            // Assets
            data.Add("|- Assets");
            data.Add("    |- Textures: " + AssetLoader.LoadedTextures.Count);
            data.AddRange(AssetLoader.LoadedTextures.Select(texture => "        |- [" + texture.Key + "] " + texture.Value.Width + "x" + texture.Value.Height));

            data.Add("    |- Fonts: " + AssetLoader.LoadedFonts.Count);
            data.AddRange(AssetLoader.LoadedFonts.Select(font => "        |- [" + font.Key + "] " + font.Value.Name));

            // Entities
            data.Add("|- Entities: " + SceneManager.CurrentScene?.RegisteredEntities.Count);
            if (SceneManager.CurrentScene?.RegisteredEntities != null)
            {
                data.AddRange(from entity in SceneManager.CurrentScene?.RegisteredEntities select "    |- [" + entity.Key + "] " + entity.Value.Components.Count + " Components");
            }

            data.Add("|- Running Systems: " + SceneManager.CurrentScene?.RunningSystems.Count);
            if (SceneManager.CurrentScene?.RunningSystems != null)
            {
                data.AddRange(from sys in SceneManager.CurrentScene?.RunningSystems select "    |- [" + sys.Priority + "] " + sys + " - " + sys.Links.Count + " Links");
            }

            Scripting.RunScript("log('" + string.Join("\\n", data) + "');");
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