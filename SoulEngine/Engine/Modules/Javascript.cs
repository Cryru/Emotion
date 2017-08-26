// SoulEngine - https://github.com/Cryru/SoulEngine

using System;
using System.Collections.Generic;
using Jint.Native;
using Soul.Engine.Interfaces;

namespace Soul.Engine.Modules
{
    public class Javascript : IScriptEngine
    {
        #region Declarations
        /// <summary>
        /// The Jint-Javascript engine.
        /// </summary>
        public static Jint.Engine Interpreter;
        #endregion

        public Javascript()
        {
            // Define the Jint engine.
            Interpreter = new Jint.Engine();
        }

        public void Expose(string name, object exposedData)
        {
            Interpreter.SetValue(name, exposedData);
        }

        public object RunScript(string script)
        {
            try
            {
                // Run the script and append a return if specified.
                return Interpreter.Execute(script).GetCompletionValue();
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