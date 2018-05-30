// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Emotion.Engine;
using Emotion.Game.Camera;
using Emotion.GLES;
using Emotion.Primitives;
using Soul.Logging;

#endregion

namespace Emotion.Debug
{
    public static class Debugger
    {
        #region Properties

        /// <summary>
        /// Sources which to log.
        /// </summary>
        public static List<MessageSource> SourceFilter;

        /// <summary>
        /// Types to log.
        /// </summary>
        public static List<MessageType> TypeFilter;

        #endregion

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
            SourceFilter = new List<MessageSource>();
            TypeFilter = new List<MessageType>();
            _logger = new ImmediateLoggingService
            {
                LogLimit = 10,
                Limit = 2000,
                Stamp = "Emotion Engine Log"
            };
            _mutexLock = new object();
            _command = "";

            // Populate filters.
            MessageSource[] allSources = (MessageSource[]) Enum.GetValues(typeof(MessageSource));
            foreach (MessageSource source in allSources)
            {
                SourceFilter.Add(source);
            }

            MessageType[] allTypes = (MessageType[]) Enum.GetValues(typeof(MessageType));
            foreach (MessageType type in allTypes)
            {
                TypeFilter.Add(type);
            }

            // Remove spam.
            TypeFilter.Remove(MessageType.Trace);


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
            // Check against filters.
            if (TypeFilter.IndexOf(type) == -1 || SourceFilter.IndexOf(source) == -1) return;

            // Prevent logging from multiple threads messing up coloring and logging.
            lock (_mutexLock)
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
                Console.WriteLine("[" + source + "] " + message);

                // Restore the normal color.
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
            }
        }

        #region Loops

        /// <summary>
        /// Is run every tick by the platform context.
        /// </summary>
        [Conditional("DEBUG")]
        internal static void Update(ScriptingEngine scripting)
        {
            // Check if there is a command to execute.
            if (_command == string.Empty) return;
            scripting.RunScript(_command);
            _command = "";
        }

        [Conditional("DEBUG")]
        internal static void DebugDraw(Context context)
        {
            // Check if there is an attached renderer with a camera.
            if (context.Renderer?.Camera != null) CameraBoundDraw(context.Renderer);

            // Draw the mouse cursor location.
            MouseBoundDraw(context.Renderer, context.Input);
        }

        /// <summary>
        /// Processes console input without blocking the engine.
        /// </summary>
        [Conditional("DEBUG")]
        private static void ConsoleThread()
        {
            while (!Environment.HasShutdownStarted)
            {
                string readLine = Console.ReadLine();
                if (readLine != null) _command = readLine.Trim(' ');
            }
        }

        #endregion

        #region Debug Drawing

        [Conditional("DEBUG")]
        private static void CameraBoundDraw(Renderer renderer)
        {
            CameraBase camera = renderer.Camera;

            // Draw bounds.
            renderer.DrawRectangleOutline(camera.Bounds, Color.Yellow);

            // Draw center.
            Rectangle centerDraw = new Rectangle(0, 0, 10, 10);
            centerDraw.X = (int) camera.Bounds.Center.X - centerDraw.Width / 2;
            centerDraw.Y = (int) camera.Bounds.Center.Y - centerDraw.Height / 2;

            renderer.DrawRectangleOutline(centerDraw, Color.Yellow);
        }

        [Conditional("DEBUG")]
        private static void MouseBoundDraw(Renderer renderer, Input.Input input)
        {
            Vector2 mouseLocation = input.GetMousePosition();

            Rectangle mouseBounds = new Rectangle(0, 0, 10, 10);
            mouseBounds.X = (int) mouseLocation.X - mouseBounds.Width / 2;
            mouseBounds.Y = (int) mouseLocation.Y - mouseBounds.Height / 2;

            renderer.DrawRectangleOutline(mouseBounds, Color.Pink, false);
        }

        #endregion
    }
}