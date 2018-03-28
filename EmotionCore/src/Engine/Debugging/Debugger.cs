// Emotion - https://github.com/Cryru/Emotion

#if DEBUG

#region Using

using System;
using System.Diagnostics;
using System.Threading;
using Emotion.Platform;
using Emotion.Primitives;
using Soul.Logging;

#endregion

namespace Emotion.Engine.Debugging
{
    public static class Debugger
    {
        #region Declarations

        /// <summary>
        /// A Soul.Logging service which logs all debug messages to a file.
        /// </summary>
        private static ImmediateLoggingService _logger = new ImmediateLoggingService
        {
            LogLimit = 10,
            Limit = 2000,
            Stamp = "Emotion Engine Log"
        };

        /// <summary>
        /// An empty object used as a mutex when writing log messages.
        /// </summary>
        private static object _mutexLock = new object();

        /// <summary>
        /// The next debug command to process.
        /// </summary>
        private static string _command = "";

        /// <summary>
        /// The process handle of the application.
        /// </summary>
        private static Process _currentProcess = Process.GetCurrentProcess();

        #endregion

        /// <summary>
        /// Setup the debugger.
        /// </summary>
        static Debugger()
        {
            // Start the console thread.
            Thread consoleThread = new Thread(ConsoleThread);
            consoleThread.Start();
            while (!consoleThread.IsAlive)
            {
            }
        }

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="type">The type of message to log.</param>
        /// <param name="source">The source of the message.</param>
        /// <param name="message">The message itself.</param>
        public static void Log(MessageType type, MessageSource source, string message)
        {
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
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                    case MessageType.Trace:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case MessageType.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                }

                // Log and display the message.
                _logger.Log("[" + type + "-" + source + "] " + message);
                Console.WriteLine(message);

                // Restore the normal color.
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
            }
        }

        /// <summary>
        /// Is run every tick by the platform context.
        /// </summary>
        /// <param name="context">The context updating the debugger.</param>
        public static void DebugLoop(Context context)
        {
            // Check if there is a command to execute.
            if (_command != string.Empty)
            {
                context.ScriptingEngine.RunScript(_command);
                _command = "";
            }

            // Check if there is an attached renderer with a camera.
            if (context.Renderer?.Camera != null) CameraBoundDraw(context.Renderer);

            // Draw the mouse cursor location.
            MouseBoundDraw(context.Renderer, context.Input);
        }

        #region Debug Drawing

        private static void CameraBoundDraw(Renderer renderer)
        {
            // Draw bounds.
            renderer.DrawRectangle(renderer.Camera.Bounds, Color.Yellow);

            // Draw center.
            Rectangle centerDraw = new Rectangle(0, 0, 10, 10);
            centerDraw.X = (int) renderer.Camera.Bounds.Center.X - centerDraw.Width / 2;
            centerDraw.Y = (int) renderer.Camera.Bounds.Center.Y - centerDraw.Height / 2;

            renderer.DrawRectangle(centerDraw, Color.Yellow);
        }

        private static void MouseBoundDraw(Renderer renderer, Input input)
        {
            Vector2 mouseLocation = input.GetMousePosition();

            Rectangle mouseBounds = new Rectangle(0, 0, 10, 10);
            mouseBounds.X = (int) mouseLocation.X - mouseBounds.Width / 2;
            mouseBounds.Y = (int) mouseLocation.Y - mouseBounds.Height / 2;

            renderer.DrawRectangle(mouseBounds, Color.Pink, false);
        }

        #endregion

        #region Scripting

        /// <summary>
        /// Processes console input without blocking the engine.
        /// </summary>
        private static void ConsoleThread()
        {
            while (!_currentProcess.HasExited)
            {
                string readLine = Console.ReadLine();
                if (readLine != null) _command = readLine.Trim(' ');
            }
        }

        #endregion
    }
}
#endif