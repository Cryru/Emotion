// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using Soul.Engine.Diagnostics;
using Soul.Engine.Enums;

#endregion

namespace Soul.Engine.Modules
{
    /// <summary>
    /// The scripting engine host.
    /// </summary>
    public static class Scripting
    {
        /// <summary>
        /// The Jint-Javascript engine.
        /// </summary>
        internal static Jint.Engine Interpreter;

        /// <summary>
        /// A list of registered actions.
        /// </summary>
        private static List<string> _registeredActions;

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

            _registeredActions = new List<string>();

            // Expose the systemic script functions.
            Expose("register", (Func<string, int>)Register);
            Expose("unregister", (Action<int>)Unregister);
        }

        internal static void Update()
        {
            // Run all registered actions.
            foreach (string script in _registeredActions)
            {
                Interpreter.Execute(script);
            }
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
        /// <param name="safe">Whether to run the script safely.</param>
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
                    Debugging.DebugMessage(DiagnosticMessageType.Scripting, scriptResponse.ToString());
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
                    Debugging.DebugMessage(DiagnosticMessageType.Error, "A script has timed out.");
                    Debugging.DebugMessage(DiagnosticMessageType.Scripting, " " + script);
#endif
                    return null;
                }

#if !DEBUG
                // Raise a scripting error.
                ErrorHandling.Raise(DiagnosticMessageType.Scripting, e.Message);
#else
                // Report error.
                Debugging.DebugMessage(DiagnosticMessageType.Error, e.Message);

#endif
                return null;
            }
        }

        /// <summary>
        /// Registers a function to be executed every frame.
        /// </summary>
        /// <param name="function">The script to register.</param>
        /// <returns>The index for the function.</returns>
        public static int Register(string function)
        {
            // Check if the function has already been added.
            if (_registeredActions.IndexOf(function) != -1) return -1;

            // Get the future index.
            int futureIndex = _registeredActions.Count;

#if DEBUG
            Debugging.DebugMessage(DiagnosticMessageType.Scripting, "Registered script (" + futureIndex + ") {" + function.Replace("\n", "").Replace(" ", "") + "}");
#endif

            // Add the function to the list.
            _registeredActions.Add(function);

            // Return the index.
            return futureIndex;
        }

        /// <summary>
        /// Unregisters a function to no longer be executed every frame.
        /// </summary>
        /// <param name="index">The index of the function to unregister.</param>
        private static void Unregister(int index)
        {
            if (index < 0) return;
            if (index > _registeredActions.Count - 1) return;

            _registeredActions[index] = null;
        }

        #endregion
    }
}