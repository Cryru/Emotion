#region Using

using System;
using System.Diagnostics;

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
            // Log the error.
            Engine.Log.Error(ex);

            // Check if suppressing errors.
            if (SuppressErrors) return;

            // If the debugger is attached, break so the error can be inspected.
            if (Debugger.IsAttached) Debugger.Break();

            // Display the error box.
            Engine.Host.DisplayErrorMessage($"Fatal error occured!\n{ex}");

            // Check whether to crash - and do so.
            if (Engine.Flags.CrashOnError) Engine.Quit();
        }
    }
}