// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Soul.Engine.Enums;
using Soul.Engine.Internal;

#endregion

namespace Soul.Engine.Modules
{
    public static class ScriptEngine
    {
        #region Declarations

        /// <summary>
        /// The Jint-Javascript engine.
        /// </summary>
        public static Jint.Engine Interpreter;

        /// <summary>
        /// Currently running scripts.
        /// </summary>
        private static List<AsyncScript> _activeScripts;

        /// <summary>
        /// A list of registered actions.
        /// </summary>
        private static List<Action> _registeredActions;

        /// <summary>
        /// Any script threads which take more milliseconds that specified here will be terminated.
        /// </summary>
        private static int threadTimeout = 300;

        #endregion

        public static void Start()
        {
            // Define the Jint engine.
            Interpreter = new Jint.Engine();

            // Define a list of script threads.
            _activeScripts = new List<AsyncScript>();

            // Expose the list of threads.
            Expose("threads", _activeScripts);

            // Define a list of registered actions.
            _registeredActions = new List<Action>();

            // Expose the registered functions functions.
            Expose("register", (Func<Action, int>)Register);
            Expose("unregister", (Action<int>)Unregister);
        }

        public static void Update()
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

            lock (_registeredActions)
            {
                // Run all registered actions.
                foreach (Action a in _registeredActions)
                {
                    a?.Invoke();
                }
            }
  
        }

        /// <summary>
        /// Exposes an object or function to be accessible from inside the scripting engine.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="exposedData"></param>
        public static void Expose(string name, object exposedData)
        {
            Interpreter?.SetValue(name, exposedData);
        }

        /// <summary>
        /// Executes the provided string on the Javascript engine asynchronously.
        /// </summary>
        /// <param name="script">The script to execute.</param>
        /// <param name="looping">Whether the script is looping and shouldn't timeout.</param>
        /// <returns></returns>
        public static void RunScriptAsync(string script, bool looping = false)
        {
            // Create an async script and add it to the managing list.
            _activeScripts?.Add(new AsyncScript(script, threadTimeout, looping));
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

                // If it isn't empty log it.
                if (scriptResponse != null)
                    Debugger.DebugMessage(DebugMessageSource.ScriptModule, scriptResponse.ToString());

                // Return the response.
                return scriptResponse;
            }
            catch (Exception e)
            {
                // Check if thread error, which we handle inside the AsyncScript object with a clearer error.
                if (e.Message == "Thread was being aborted.")
                {
                    return null;
                }

                // Raise a scripting error.
                Error.Raise(50, e.Message);
                return null;
            }
        }

        #region Functions

        /// <summary>
        /// Registers a function to be executed every frame.
        /// </summary>
        /// <param name="function">The function to register.</param>
        /// <returns>The index for the function.</returns>
        private static int Register(Action function)
        {
            lock (_registeredActions)
            {
                // Check if the function has already been added.
                if (_registeredActions.IndexOf(function) != -1) return -1;

                // Get the future index.
                int futureIndex = _registeredActions.Count;

                // Add the function to the list.
                _registeredActions.Add(function);

                // Return the index.
                return futureIndex;
            }
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