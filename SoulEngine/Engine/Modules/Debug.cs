// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Threading;
using Soul.Engine.ECS;

#endregion

namespace Soul.Engine.Modules
{
    public class Debug : Actor
    {
        #region Variables

        /// <summary>
        /// The next debug command to process.
        /// </summary>
        private string _command;

        #endregion

        /// <summary>
        /// Initializes debug logic.
        /// </summary>
        public override void Initialize()
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
        }

        /// <summary>
        /// Updates debug logic.
        /// </summary>
        public override void Update()
        {
#if !DEBUG
            return;
#endif

            // Check if there is a command to execute.
            if (_command != string.Empty)
            {
                // Get the response of the command from the current script engine.
                object scriptResponse = Settings.ScriptEngine.RunScript(_command);

                // If any response write it.
                if (scriptResponse != null) DebugMessage("Script", scriptResponse.ToString());

                // Clear the command.
                _command = "";
            }
        }

        /// <summary>
        /// Processes console input without blocking the engine.
        /// </summary>
        private void ConsoleThread()
        {
            while (Raya.System.Context.Current.Running)
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
        public static void DebugMessage(string source, string message)
        {
#if !DEBUG
            return;
#endif
            // Check source to color.
            switch (source)
            {
                case "Error":
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }

            Console.WriteLine(Functions.FormatDebugMessage(source, message));

            // Restore colors.
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}