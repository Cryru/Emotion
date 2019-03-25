#region Using

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Adfectus.Logging;

#endregion

namespace Adfectus.Common
{
    public static class ErrorHandler
    {
        /// <summary>
        /// Whether to suppress any and all errors. False by default.
        /// </summary>
        public static bool SuppressErrors { get; set; }

        /// <summary>
        /// Setup the error handler. Is done by the engine at the start.
        /// </summary>
        internal static void Setup()
        {
            // Attach to unhandled exceptions if the debugger is not attached.
            if (!Debugger.IsAttached) AppDomain.CurrentDomain.UnhandledException += (e, a) => { SubmitError((Exception) a.ExceptionObject); };
        }

        /// <summary>
        /// Submit that an error has happened. Handles logging and closing of the engine safely.
        /// </summary>
        /// <param name="ex">The exception connected with the error occured.</param>
        public static void SubmitError(Exception ex)
        {
            // Check if suppressing errors.
            if (SuppressErrors) return;

            // If the debugger is attached, break so the error can be inspected.
            if (Debugger.IsAttached) Debugger.Break();

            // Log the error.
            Engine.Log.Error(ex);

            // Display the message box.
            ShowErrorBox($"Fatal error occured!\n{ex}");

            // Check whether to crash - and do so.
            if (Engine.Flags.CrashOnError) Engine.Quit();
        }

        /// <summary>
        /// Display an error message as a message box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public static void ShowErrorBox(string message)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    WindowsNative.MessageBox(IntPtr.Zero, message, "Something went wrong!", (uint) (0x00000000L | 0x00000010L));
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    ExecuteBashCommand($"osascript -e 'tell app \"System Events\" to display dialog \"{message}\" buttons {{\"OK\"}} with icon caution'");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    try
                    {
                        // Display a message box using Zenity.
                        ExecuteBashCommand($"zenity --error --text=\"{message}\" --title=\"Something went wrong!\" 2>/dev/null");
                    }
                    catch (Exception)
                    {
                        // Fallback to xmessage.
                        ExecuteBashCommand($"xmessage \"{message}\"");
                    }
                }
            }
            catch (Exception e)
            {
                Engine.Log.Warning($"Couldn't display error message box - {message}. {e}", MessageSource.Other);
            }
        }

        /// <summary>
        /// Execute a bash command on Unix systems.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        private static void ExecuteBashCommand(string command)
        {
            Process msgBox = new Process();
            msgBox.StartInfo.FileName = "/bin/bash";
            msgBox.StartInfo.Arguments = $"-c \"{command.Replace("\"", "\\\"")}\"";
            msgBox.StartInfo.UseShellExecute = false;
            msgBox.StartInfo.CreateNoWindow = true;
            msgBox.Start();
        }
    }
}