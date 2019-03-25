#region Using

using System;
using System.Collections.Concurrent;
using Adfectus.Logging;
using Jint.Parser;

#if DEBUG

#endif

#endregion

namespace Adfectus.Common
{
    /// <summary>
    /// A Javascript engine used to execute code at runtime.
    /// </summary>
    public sealed class ScriptingEngine
    {
        #region Declarations

        /// <summary>
        /// An instance of a JS interpreter.
        /// </summary>
        private Jint.Engine _interpreter;

        /// <summary>
        /// Exposed properties.
        /// </summary>
        private ConcurrentBag<string> _exposedProperties = new ConcurrentBag<string>();

        #endregion

        /// <summary>
        /// Initializes the scripting engine, setting up Jint.
        /// </summary>
        internal ScriptingEngine(TimeSpan timeout)
        {
            // Define the Jint engine.
            _interpreter = new Jint.Engine(opts =>
            {
                // Set scripting timeout.
                opts.TimeoutInterval(timeout);
#if DEBUG
                // Enable scripting debugging.
                opts.DebugMode();
                opts.AllowDebuggerStatement();
#endif
            });

            Expose("help", (Func<string>) (() => "\n---Exposed Functions---\n" + string.Join("\n", _exposedProperties)), "Prints all exposed properties and their descriptions.");
        }

        #region Functions

        /// <summary>
        /// Exposes an object or function to be accessible from inside the scripting engine.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="exposedData">The data to expose.</param>
        /// <param name="description">Optional description of what you are exposing.</param>
        public void Expose(string name, object exposedData, string description = "")
        {
            _interpreter.SetValue(name, exposedData);
            _exposedProperties.Add(name + " - " + description);
        }

        /// <summary>
        /// Executes the provided string on the Javascript engine.
        /// </summary>
        /// <param name="script">The script to execute.</param>
        /// <param name="safe">Whether to run the script safely.</param>
        /// <returns></returns>
        public object RunScript(string script, bool safe = true)
        {
            if (safe) script = "(function () { " + script + " })()";

            try
            {
                // Run the script and get the response.
                ParserOptions parser = new ParserOptions
                {
                    Source = script
                };

                object scriptResponse = _interpreter.Execute(script, parser).GetCompletionValue();

                // If it isn't empty log it.
                if (scriptResponse != null)
                    Engine.Log.Trace($"Script executed, result: ${scriptResponse}", MessageSource.ScriptingEngine);

                // Return the response.
                return scriptResponse;
            }
            catch (Exception ex)
            {
                // Check if timeout, and if not throw an exception.
                if (ex.Message != "The operation has timed out." && Engine.Flags.StrictScripts)
                    ErrorHandler.SubmitError(new Exception($"Scripting error in script: [{script}]", ex));
                else
                    Engine.Log.Warning($"Scripting error in script: [{script}]\n{ex}", MessageSource.ScriptingEngine);

                return null;
            }
        }

        #endregion
    }
}