// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

#if !DEBUG
using System;
using System.IO;
#endif

using System;
using System.IO;
using Soul.Engine.Diagnostics;
using Soul.Engine.Enums;
using Soul.IO;

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
        internal static void Raise(DiagnosticMessageType origin, string errorMessage)
        {
            string errorFormatted = "(" + origin + ") " + errorMessage;

            // Write a crash report.
            Directory.CreateDirectory("Errors");

            string report = "";

            // Collect statistics.
            AddReportHeader("Statistics Dump", ScriptLibrary.Statistics(), ref report);

            // Add entities
            AddReportHeader("Entities Dump", ScriptLibrary.GetEntities(), ref report);

            // Add error.
            AddReportHeader("Crashing Error", errorFormatted, ref report);

            Write.File("Errors" + Path.DirectorySeparatorChar + "CrashReport_ " + DateTime.Now.ToFileTime(),
                report);

            // Close the engine.
            Core.Stop();
        }

        private static void AddReportHeader(string headerName, string headerData, ref string report)
        {
            report += "\n----------------------------------------\n" + headerName +
                      ":\n----------------------------------------\n" + headerData;
        }
    }
}