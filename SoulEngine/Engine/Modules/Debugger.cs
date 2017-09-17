// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using Raya.System;
using Soul.Engine.Enums;

#endregion

#pragma warning disable 4014

namespace Soul.Engine.Modules
{
    public static class Debugger
    {
        #region Declarations

        /// <summary>
        /// The next debug command to process.
        /// </summary>
        private static string _command;

        #endregion

        /// <summary>
        /// Initializes debug logic.
        /// </summary>
        public static void Start()
        {
            // If not debugging exit.
#if !DEBUG
            return;
#endif
            // Assign command.
            _command = "";

            // Start the console thread.
            Thread consoleThread = new Thread(ConsoleThread);
            consoleThread.Start();

            // Expose debugging script functions.
            ScriptEngine.Expose("reflect", (Func<object, string>)Reflect);
            ScriptEngine.Expose("print", (Action<string>)Print);
            ScriptEngine.Expose("fps", (Func<string>)FPS);
        }

        /// <summary>
        /// Updates debug logic.
        /// </summary>
        public static void Update()
        {
#if !DEBUG
            return;
#endif

            // Check if there is a command to execute.
            if (_command != string.Empty)
            {
                // Run the command through the scripting module.
                ScriptEngine.RunScriptAsync(_command);

                // Clear the command.
                _command = "";
            }
        }

        /// <summary>
        /// Processes console input without blocking the engine.
        /// </summary>
        private static void ConsoleThread()
        {
            while (Context.Current.Running)
            {
                string readLine = Console.ReadLine();
                if (readLine != null) _command = readLine.Trim(' ');
            }
        }

        /// <summary>
        /// Writes a debug message.
        /// </summary>
        /// <param name="source">The source of the message.</param>
        /// <param name="message">The message itself.</param>
        public static void DebugMessage(DebugMessageSource source, string message)
        {
#if !DEBUG
            return;
#endif

            lock (message)
            {
                // Whether to skip printing the message.
                bool skipPrint = false;

                // Check source to color.
                switch (source)
                {
                    case DebugMessageSource.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case DebugMessageSource.ScriptModule:
                        // Custom print code.
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(FormatDebugMessage(source.ToString(), ""));
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write(message);
                        Console.WriteLine();
                        skipPrint = true;
                        break;
                    case DebugMessageSource.PhysicsModule:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    case DebugMessageSource.SceneManager:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }

                if (!skipPrint)
                {
                    Console.WriteLine(FormatDebugMessage(source.ToString(), message));

                    // Log the message, if it isn't an error. Errors are logged in the Error class as debugging might be turned off.
                    if (source != DebugMessageSource.Error) Logger.Add(FormatDebugMessage(source.ToString(), message));
                }

                // Restore colors.
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        #region Functions

        /// <summary>
        /// Formats a debug message.
        /// </summary>
        /// <param name="left">Message on the left, usually the source or a code.</param>
        /// <param name="right">Message on the right, usually the message itself.</param>
        /// <returns></returns>
        public static string FormatDebugMessage(string left, string right)
        {
            return "[" + left + "] " + right;
        }

        #endregion

        #region Script Debugging Functions

        /// <summary>
        /// Returns a string of all properties of an object.
        /// </summary>
        /// <param name="obj">The object to reflect.</param>
        /// <returns>A string of all object data.</returns>
        private static string Reflect(object obj)
        {
            string text = "";

            // Check if IEnumerable.
            if (obj is IEnumerable)
                text = "Cannot reflect IEnumerables.";
            else
                foreach (PropertyInfo prop in obj.GetType()
                    .GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public |
                                   BindingFlags.DeclaredOnly))
                {
                    if (text != "") text += "\n";

                    text += prop.Name + "= " + prop.GetValue(obj, null);
                }


            return text;
        }

        /// <summary>
        /// Prints a message to the console.
        /// </summary>
        /// <param name="text">The message to print.</param>
        private static void Print(string text)
        {
            DebugMessage(DebugMessageSource.ScriptModule, text);
        }

        /// <summary>
        /// Returns the current frames per second the engine is running at.
        /// </summary>
        /// <returns>The current fps.</returns>
        private static string FPS()
        {
            return (1000 / Core.FrameTime).ToString();
        }
        #endregion
    }
}