using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soul.Engine.Interfaces
{
    public interface IScriptEngine
    {
        /// <summary>
        /// Executes a script.
        /// </summary>
        /// <param name="script">The script to execute as a string.</param>
        object RunScript(string script);
        /// <summary>
        /// Exposes data to the scripting engine.
        /// </summary>
        /// <param name="name">The identifier of the data.</param>
        /// <param name="exposedData">The data object.</param>
        void Expose(string name, object exposedData);
    }
}
