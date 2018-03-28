using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if DEBUG
using Emotion.Engine.Debugging;
#endif
using Emotion.Platform;

namespace Emotion.Engine
{
    public sealed class ScriptingEngine
    {
        /// <summary>
        /// The Jint-Javascript engine.
        /// </summary>
        internal Jint.Engine Interpreter;

#region Module API

        /// <summary>
        /// Initializes the module.
        /// </summary>
        internal void Setup(Context context)
        {
            // Define the Jint engine.
            Interpreter = new Jint.Engine(opts =>
            {
                // Set scripting timeout.
                opts.TimeoutInterval(context.InitialSettings.ScriptTimeout);
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
        public void Expose(string name, object exposedData)
        {
            Interpreter.SetValue(name, exposedData);
        }

        /// <summary>
        /// Executes the provided string on the Javascript engine.
        /// </summary>
        /// <param name="script">The script to execute.</param>
        /// <param name="safe">Whether to run the script safely.</param>
        /// <returns></returns>
        public object RunScript(string script, bool safe = true)
        {
            if (safe)
            {
                script = "(function () {" + script + "})()";
            }

            try
            {
                // Run the script and get the response.
                object scriptResponse = Interpreter.Execute(script).GetCompletionValue();

#if DEBUG
                // If it isn't empty log it.
                if (scriptResponse != null)
                    Debugger.Log(MessageType.Info, MessageSource.ScriptingEngine, scriptResponse.ToString());
#endif
                // Return the response.
                return scriptResponse;
            }
            catch (Exception e)
            {
                // Check if timeout, and if not throw an exception.
                if (e.Message != "The operation has timed out.") throw e;
#if DEBUG
                Debugger.Log(MessageType.Warning, MessageSource.ScriptingEngine, "A script has timed out.");
                Debugger.Log(MessageType.Trace, MessageSource.ScriptingEngine, " " + script);
#endif
                return null;
            }
        }

#endregion
    }

}
