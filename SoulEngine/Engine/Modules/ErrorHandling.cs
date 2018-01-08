// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Breath.Systems;
using Soul.Engine.Enums;

#endregion

namespace Soul.Engine.Modules
{
    internal static class ErrorHandling
    {
        #region Module API

        /// <summary>
        /// Initializes the module.
        /// </summary>
        internal static void Setup()
        {
            // Connect to the Breath error manager.
            ErrorHandler.ErrorCallback += error => { Raise(ErrorOrigin.Breath, error); };
        }

        /// <summary>
        /// Updates the module.
        /// </summary>
        internal static void Update()
        {
        }

        #endregion

        /// <summary>
        /// Raises an error.
        /// </summary>
        /// <param name="origin">The origin of the error.</param>
        /// <param name="errorMessage">The error message.</param>
        internal static void Raise(ErrorOrigin origin, string errorMessage)
        {
            string errorFormatted = "(" + origin + ") " + errorMessage;

#if DEBUG
            // If debugging, log the error.
            Debugging.DebugMessage(DebugMessageType.Error, errorFormatted);
#else // Write a crash report.
            System.IO.Directory.CreateDirectory("Errors");
            IO.Write.File("CrashReport_ " + DateTime.Now.ToFileTime(), errorFormatted);

            // Close the engine.
            Core.Stop(true);
#endif
        }
    }
}