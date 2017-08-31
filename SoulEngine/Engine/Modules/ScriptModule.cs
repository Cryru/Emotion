// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Soul.Engine.ECS;
using Soul.Engine.Enums;
using Soul.Engine.Internal;

#endregion

namespace Soul.Engine.Modules
{
    public class ScriptModule : Actor
    {
        #region Declarations

        /// <summary>
        /// The Jint-Javascript engine.
        /// </summary>
        public static Jint.Engine Interpreter;

        /// <summary>
        /// Currently running scripts.
        /// </summary>
        private List<AsyncScript> _activeScripts;

        /// <summary>
        /// Any script threads which take more milliseconds that specified here will be terminated.
        /// </summary>
        private int threadTimeout = 300;

        #endregion

        public override void Initialize()
        {
            // Define the Jint engine.
            Interpreter = new Jint.Engine();

            // Define a list of script threads.
            _activeScripts = new List<AsyncScript>();

            // Expose the list of threads.
            Expose("threads", _activeScripts);

            // Expose context.
            Expose("context", Globals.Context);
        }

        public override void Update()
        {
            // Loop through the active script threads to do some checks.
            for (int i = _activeScripts.Count - 1; i >= 0; i--)
            {
                // If the thread is not running, remove it.
                if (!_activeScripts[i].isRunning)
                {
                    _activeScripts.RemoveAt(i);
                }
                else
                {
                    if (_activeScripts[i].UpdateTime()) _activeScripts.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Exposes an object or function to be accessible from inside the scripting engine.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="exposedData"></param>
        public void Expose(string name, object exposedData)
        {
            Interpreter.SetValue(name, exposedData);
        }

        /// <summary>
        /// Executes the provided string on the Javascript engine asynchronously.
        /// </summary>
        /// <param name="script">The script to execute.</param>
        /// <returns></returns>
        public async Task<object> RunScriptAsync(string script)
        {
            // Create a token for cancellation.
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            // Create the task.
            Task<object> scriptTask = Task<object>.Factory.StartNew(() => RunScript(script), token);

            // Add the token to the list of threads.
            _activeScripts.Add(new AsyncScript(scriptTask, tokenSource, threadTimeout));

            // Run the script asynchronously.
            return await scriptTask;
        }

        /// <summary>
        /// Executes the provided string on the Javascript engine.
        /// </summary>
        /// <param name="script">The script to execute.</param>
        /// <returns></returns>
        public object RunScript(string script)
        {
            try
            {
                // Run the script and get the response.
                object scriptResponse = Interpreter.Execute(script).GetCompletionValue();

                // If it isn't empty log it.
                if (scriptResponse != null)
                    DebugModule.DebugMessage(DebugMessageSource.ScriptModule, scriptResponse.ToString());

                // Return the response.
                return scriptResponse;
            }
            catch (Exception e)
            {
                // Raise a scripting error.
                Error.Raise(50, e.Message);
                return null;
            }
        }
    }
}