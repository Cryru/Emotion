// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Raya.Input;
using Raya.System;
using Soul.Engine.Enums;
using Soul.Engine.Objects;

#endregion

#pragma warning disable 4014
// ReSharper disable InconsistentlySynchronizedField

namespace Soul.Engine.Modules
{
    public static class Debugger
    {
        #region Declarations

        #region Manual Mode

        /// <summary>
        /// Whether manual mode is on.
        /// </summary>
        public static bool ManualMode;

        /// <summary>
        /// Whether to advance the current frame when in manual mode.
        /// </summary>
        public static bool AdvanceFrame;

        /// <summary>
        /// The current manual mode frame.
        /// </summary>
        public static int ManualModeFrame;

        #endregion

        #region Select Mode

        /// <summary>
        /// Whether we are in select mode.
        /// </summary>
        public static bool SelectMode = false;

        /// <summary>
        /// The last selected object.
        /// </summary>
        private static GameObject _lastSelected;

        #endregion



        /// <summary>
        /// The next debug command to process.
        /// </summary>
        private static string _command;

        /// <summary>
        /// Sources to hide messages from.
        /// </summary>
        public static List<DebugMessageSource> HiddenSources =
            new List<DebugMessageSource> {};

        #endregion

        /// <summary>
        /// Initializes debug logic.
        /// </summary>
        static Debugger()
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
            ScriptEngine.Expose("reflect", (Func<object, string>) Reflect);
            ScriptEngine.Expose("print", (Action<string>) Print);
            ScriptEngine.Expose("fps", (Func<string>) FPS);
            ScriptEngine.Expose("manualMode", (Action) ToggleManualMode);
            ScriptEngine.Expose("selectMode", (Func<bool>) (() =>
            {
                return SelectMode = !SelectMode;
            }));
            ScriptEngine.Expose("showSource", (Action<DebugMessageSource>) ShowMessageSource);
            ScriptEngine.Expose("hideSource", (Action<DebugMessageSource>) HideMessageSource);
            ScriptEngine.Expose("help", (Func<string, string>) DebugScriptHelp);
        }

        /// <summary>
        /// Updates the debug logic, doesn't run in release mode.
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

            // If in manual mode check for inputs.
            if (ManualMode)
            {
                // Reset frame advancement.
                AdvanceFrame = false;

                if (Input.KeyHeld(Keyboard.Key.F11) || Input.KeyPressed(Keyboard.Key.F10))
                {
                    AdvanceFrame = true;
                    ManualModeFrame++;

                    DebugMessage(DebugMessageSource.Debug, "Manual Mode Frame: " + ManualModeFrame);
                }
            }

            if (SelectMode)
            {
                GameObject mouseOvered = SceneManager.CurrentScene.GetMousedObject();

                if (mouseOvered != null)
                {
                    if (mouseOvered != _lastSelected)
                    {
                        ScriptEngine.Expose("selected", _lastSelected);
                        DebugMessage(DebugMessageSource.Debug, "You are selecting " + mouseOvered.Name + " who has " + mouseOvered.ChildrenCount + " children.");
                        _lastSelected = mouseOvered;
                    }
                }
            }

            // Print the fps in the window title.
            Core.NativeContext.Window.Title = Settings.WTitle + " FPS: " + FPS();
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

        #region Functions

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
                // Whether to skip printing the message. By default messages in the hidden array are skipped.
                bool skipPrint = HiddenSources.IndexOf(source) != -1;

                // Check source to color.
                switch (source)
                {
                    case DebugMessageSource.Debug:
                        if (skipPrint) break;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Debug Message: " + message);
                        skipPrint = true;
                        break;
                    case DebugMessageSource.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case DebugMessageSource.ScriptModule:
                        if (skipPrint) break;
                        // Custom print code.
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(FormatDebugMessage(source.ToString(), ""));
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write(message);
                        Console.WriteLine();
                        skipPrint = true;
                        break;
                    case DebugMessageSource.AssetLoader:
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

        /// <summary>
        /// Hide the specified message source from being shown in the console.
        /// </summary>
        /// <param name="source">The message source to hide.</param>
        public static void HideMessageSource(DebugMessageSource source)
        {
            if (HiddenSources.IndexOf(source) != -1) return;

            HiddenSources.Add(source);
        }

        /// <summary>
        /// Show the specified message source in the console.
        /// </summary>
        /// <param name="source">The message source to show.</param>
        public static void ShowMessageSource(DebugMessageSource source)
        {
            if (HiddenSources.IndexOf(source) == -1) return;

            HiddenSources.Remove(source);
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
            // If paused or the frame time is zero then we are paused.
            if (Core.Paused || Core.FrameTime == 0) return "Paused";

            return (1000 / Core.FrameTime).ToString();
        }

        /// <summary>
        /// Activates manual loop mode.
        /// </summary>
        /// <returns></returns>
        private static void ToggleManualMode()
        {
            ManualMode = !ManualMode;

            if (ManualMode)
            {
                // Reset manual mode frame.
                ManualModeFrame = 0;

                DebugMessage(DebugMessageSource.Debug, "Manual mode activated.");
                DebugMessage(DebugMessageSource.Debug, "Hold F11 to loop.");
                DebugMessage(DebugMessageSource.Debug, "Press F10 to advance by one frame.");
            }
            else
            {
                DebugMessage(DebugMessageSource.Debug, "Manual mode deactivated.");
            }
        }

        #endregion

        #region Help Menu

        private static string DebugScriptHelp(string query)
        {
            switch (query)
            {
                case "message sources":
                case "source":
                case "sources":
                case "debug sources":
                case "hideSource":
                case "showSource":
                    return "You can hide or show message sources using 'hideSource' and 'showSource' respectively. The sources are as follows: " + Help_GetMessageSources();
            }

            return query + " is not a valid query. Try the name of a function." + "\n" +
                "showSource / hideSource - Display/Hide debug message sources.";
        }

        private static string Help_GetMessageSources()
        {
            string formatted = "\n";

            for (int i = 0; i < Enum.GetValues(typeof(DebugMessageSource)).Length; i++)
            {
                formatted += i + " " + Enum.GetName(typeof(DebugMessageSource), i) + "\n";
            }

            return formatted;
        }

        #endregion
    }
}