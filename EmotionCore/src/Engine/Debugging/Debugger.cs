// Emotion - https://github.com/Cryru/Emotion

#if DEBUG

#region Using

using System;
using System.Diagnostics;
using System.Threading;
using Emotion.Game.Objects.Camera;
using Emotion.Platform.Base;
using Emotion.Primitives;
using Soul.Logging;

#endregion

namespace Emotion.Engine.Debugging
{
    public class Debugger
    {
        #region Declarations

        /// <summary>
        /// A Soul.Logging service which logs all debug messages to a file.
        /// </summary>
        private ImmediateLoggingService _logger = new ImmediateLoggingService
        {
            LogLimit = 10,
            Limit = 2000,
            Stamp = "Emotion Engine Log"
        };

        /// <summary>
        /// An empty object used as a mutex when writing log messages.
        /// </summary>
        private object _mutexLock = new object();

        /// <summary>
        /// The next debug command to process.
        /// </summary>
        private string _command = "";

        /// <summary>
        /// The process handle of the application.
        /// </summary>
        private Process _currentProcess = Process.GetCurrentProcess();

        /// <summary>
        /// The context the debugger is under.
        /// </summary>
        private Context _context;

        #endregion

        /// <summary>
        /// Setup the debugger.
        /// </summary>
        public Debugger(Context context)
        {
            _context = context;

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
        public void Log(MessageType type, MessageSource source, string message)
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
                _logger.Log( "[" + type + "-" + source + "] " + message);
                Console.WriteLine( "[" + source + "] " + message);

                // Restore the normal color.
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
            }
        }

        /// <summary>
        /// Is run every tick by the platform context.
        /// </summary>
        public void DebugLoop()
        {
            // Check if there is a command to execute.
            if (_command != string.Empty)
            {
                _context.ScriptingEngine.RunScript(_command);
                _command = "";
            }

            // Check if there is an attached renderer with a camera.
            if (_context.Renderer?.GetCamera() != null) CameraBoundDraw(_context.Renderer);

            // Draw the mouse cursor location.
            MouseBoundDraw(_context.Renderer, _context.Input);
        }

        #region Debug Drawing

        private static void CameraBoundDraw(IRenderer renderer)
        {
            CameraBase camera = renderer.GetCamera();

            // Draw bounds.
            renderer.DrawRectangleOutline(camera.Bounds, Color.Yellow);

            // Draw center.
            Rectangle centerDraw = new Rectangle(0, 0, 10, 10);
            centerDraw.X = (int) camera.Bounds.Center.X - centerDraw.Width / 2;
            centerDraw.Y = (int) camera.Bounds.Center.Y - centerDraw.Height / 2;

            renderer.DrawRectangleOutline(centerDraw, Color.Yellow);
        }

        private static void MouseBoundDraw(IRenderer renderer, IInput input)
        {
            Vector2 mouseLocation = input.GetMousePosition();

            Rectangle mouseBounds = new Rectangle(0, 0, 10, 10);
            mouseBounds.X = (int) mouseLocation.X - mouseBounds.Width / 2;
            mouseBounds.Y = (int) mouseLocation.Y - mouseBounds.Height / 2;

            renderer.DrawRectangleOutline(mouseBounds, Color.Pink, false);
        }

        #endregion

        #region Scripting

        /// <summary>
        /// Processes console input without blocking the engine.
        /// </summary>
        private void ConsoleThread()
        {
            while (_context.Running)
            {
                string readLine = Console.ReadLine();
                if (readLine != null) _command = readLine.Trim(' ');
            }
        }

        #endregion
    }
}
#endif