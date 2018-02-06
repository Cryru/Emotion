// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Breath.Systems;
using Soul.Engine.Enums;

#endregion

namespace Soul.Engine.Modules
{
    internal static class ErrorHandling
    {
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