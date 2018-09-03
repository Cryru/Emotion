// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using Emotion.Debug;

#if DEBUG

#endif

#endregion

namespace Emotion.Engine
{
    public sealed class ScriptingEngine : ContextObject
    {
        #region Declarations

        /// <summary>
        /// An instance of a JS interpreter.
        /// </summary>
        private Jint.Engine _interpreter;

        /// <summary>
        /// Exposed properties.
        /// </summary>
        private List<string> _exposedProperties = new List<string>();

        #endregion

        /// <summary>
        /// Initializes the module.
        /// </summary>
        /// <param name="context">The context the scripting engine will run under.</param>
        internal ScriptingEngine(Context context) : base(context)
        {
            // Define the Jint engine.
            _interpreter = new Jint.Engine(opts =>
            {
                // Set scripting timeout.
                opts.TimeoutInterval(Context.Settings.ScriptTimeout);
#if DEBUG
                // Enable scripting debugging.
                opts.DebugMode();
                opts.AllowDebuggerStatement();
#endif
            });

            Expose("help", (Func<string>) (() => "\n---Exposed Functions---" + string.Join("\n", _exposedProperties)), "Prints all exposed properties and their descriptions.");
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
            if (safe) script = "(function () { return " + script + "; })()";

            try
            {
                // Run the script and get the response.
                object scriptResponse = _interpreter.Execute(script).GetCompletionValue();

                // If it isn't empty log it.
                if (scriptResponse != null)
                    Debugger.Log(MessageType.Info, MessageSource.ScriptingEngine, "Script executed, result: " + scriptResponse);

                // Return the response.
                return scriptResponse;
            }
            catch (Exception e)
            {
                // Check if timeout, and if not throw an exception.
                if (e.Message != "The operation has timed out." && Context.Settings.StrictScripts) throw e;

                Debugger.Log(MessageType.Warning, MessageSource.ScriptingEngine, "Scripting error: " + e.Message);
                Debugger.Log(MessageType.Trace, MessageSource.ScriptingEngine, " " + script);

                return null;
            }
        }

        #endregion
    }
}