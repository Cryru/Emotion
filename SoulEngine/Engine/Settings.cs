// SoulEngine - https://github.com/Cryru/SoulEngine

using Soul.Engine.Interfaces;
using Soul.Engine.Modules;

namespace Soul.Engine
{
    public static class Settings
    {
        static Settings()
        {
            // If no script engine is defined, default to JavaScript.
            if (ScriptEngine == null) ScriptEngine = new Javascript();
        }

        /// <summary>
        /// The scripting engine to use.
        /// </summary>
        public static IScriptEngine ScriptEngine;
    }
}