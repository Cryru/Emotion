// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Diagnostics;
using System.Threading;
using Emotion.Engine;
using Emotion.Graphics;
using Emotion.Graphics.GLES;
using Emotion.Primitives;
using Emotion.Utils;
using Soul.Logging;

#endregion

namespace Emotion.Debug
{
    public static class Debugger
    {
        #region Declarations

        /// <summary>
        /// A Soul.Logging service which logs all debug messages to a file.
        /// </summary>
        private static ImmediateLoggingService _logger;

        /// <summary>
        /// An empty object used as a mutex when writing log messages.
        /// </summary>
        private static object _mutexLock;

        /// <summary>
        /// The next debug command to process.
        /// </summary>
        private static string _command;

        #endregion

        static Debugger()
        {
#if DEBUG
            // Init.
            _logger = new ImmediateLoggingService
            {
                LogLimit = 10,
                Limit = 2000,
                Stamp = "Emotion Engine Log"
            };
            _mutexLock = new object();
            _command = "";

            // Start the console thread.
            Thread consoleThread = new Thread(() => ConsoleThread());
            consoleThread.Start();
            while (!consoleThread.IsAlive)
            {
            }
#endif
        }

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="type">The type of message to log.</param>
        /// <param name="source">The source of the message.</param>
        /// <param name="message">The message itself.</param>
        [Conditional("DEBUG")]
        public static void Log(MessageType type, MessageSource source, string message)
        {
            // Prevent logging from multiple threads messing up coloring and logging.
            lock (_mutexLock)
            {
                try
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
                    _logger.Log("[" + type + "-" + source + "] " + message);
                    if (type != MessageType.Trace) Console.WriteLine("[" + source + "] " + message);

                    // Restore the normal color.
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                catch (Exception)
                {
                    Log(MessageType.Error, MessageSource.Debugger, "Debugger logger has crashed.");
                    _logger.Log("[" + type + "-" + source + "] " + message);
                }
            }
        }

        #region Loops

        /// <summary>
        /// Is run every tick by the platform context.
        /// </summary>
        [Conditional("DEBUG")]
        internal static void Update(Context context)
        {
            // Check if there is a command to execute.
            if (_command == string.Empty) return;
            context.ScriptingEngine.RunScript(_command);
            _command = "";
        }

        [Conditional("DEBUG")]
        internal static void DebugDraw(Context context)
        {
            // Draw the mouse cursor location.
            MouseBoundDraw(context.Renderer, context.Input);
        }

        /// <summary>
        /// Processes console input without blocking the engine.
        /// </summary>
        [Conditional("DEBUG")]
        private static void ConsoleThread()
        {
            // Delay console thread on other OS
            if (CurrentPlatform.OS != PlatformID.Win32NT) Thread.Sleep(2000);

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
                Log(MessageType.Error, MessageSource.Debugger, "Debugger console thread has crashed.");
            }
        }

        #endregion

        #region Debug Drawing

        [Conditional("DEBUG")]
        private static void MouseBoundDraw(Renderer renderer, Input.Input input)
        {
            Vector2 mouseLocation = input.GetMousePosition();
            mouseLocation.X -= 5;
            mouseLocation.Y -= 5;

            renderer.DisableViewMatrix();
            renderer.RenderOutline(new Vector3(mouseLocation.X, mouseLocation.Y, 100), new Vector2(10, 10), Color.Pink);
            renderer.SyncShader(ShaderProgram.Current);
        }

        #endregion
    }
}