// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Soul.Engine.Enums;

#endregion

namespace Soul.Engine.Modules
{
    /// <summary>
    /// The scripting engine host.
    /// </summary>
    internal static class Scripting
    {
        /// <summary>
        /// The Jint-Javascript engine.
        /// </summary>
        internal static Jint.Engine Interpreter;

        #region Module API

        /// <summary>
        /// Initializes the module.
        /// </summary>
        internal static void Setup()
        {
            // Define the Jint engine.
            Interpreter = new Jint.Engine(opts =>
            {
                // Set scripting timeout.
                opts.TimeoutInterval(Settings.ScriptTimeout);
#if DEBUG
                // Enable scripting debugging.
                opts.DebugMode();
                opts.AllowDebuggerStatement();
#endif
            });
        }

        #endregion

        #region Functions

        /// <summary>
        /// Exposes an object or function to be accessible from inside the scripting engine.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="exposedData">The data to expose.</param>
        public static void Expose(string name, object exposedData)
        {
            Interpreter.SetValue(name, exposedData);
        }

        /// <summary>
        /// Executes the provided string on the Javascript engine.
        /// </summary>
        /// <param name="script">The script to execute.</param>
        /// <returns></returns>
        public static object RunScript(string script)
        {
            try
            {
                // Run the script and get the response.
                object scriptResponse = Interpreter.Execute(script).GetCompletionValue();

#if DEBUG
                // If it isn't empty log it.
                if (scriptResponse != null)
                    Debugging.DebugMessage(DebugMessageType.Info, scriptResponse.ToString());
#endif
                // Return the response.
                return scriptResponse;
            }
            catch (Exception e)
            {
                // Check if timeout.
                if (e.Message == "The operation has timed out.")
                {
#if DEBUG
                    Debugging.DebugMessage(DebugMessageType.Warning, "A script has timed out.");
                    Debugging.DebugMessage(DebugMessageType.Warning, " " + script);
#endif
                    return null;
                }

                // Raise a scripting error.
                ErrorHandling.Raise(ErrorOrigin.Scripting, e.Message);
                return null;
            }
        }

        #endregion
    }
}