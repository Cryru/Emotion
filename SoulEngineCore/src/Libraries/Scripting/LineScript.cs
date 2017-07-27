using SoulEngine.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Scripting
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A script to be executed line by line.
    /// </summary>
    public class LineScript
    {
        #region "Declarations"
        /// <summary>
        /// The current line.
        /// </summary>
        public int Pointer = 0;
        /// <summary>
        /// The script file.
        /// </summary>
        private string[] Script;
        /// <summary>
        /// If reached the last line.
        /// </summary>
        public bool Finished
        {
            get
            {
                return Pointer == Script.Length;
            }
        }
        #endregion

        public LineScript(string[] fileContents, int Pointer)
        {
            this.Pointer = Pointer;
            Script = fileContents;

            if (Pointer != 0)
            {
                for (int i = 0; i < Pointer; i++)
                {
                    if (Script.Length - 1 > i) Context.Core.Module<ScriptEngine>().ExecuteScript(Script[i]);
                }
            }
        }

        /// <summary>
        /// Executes the next line.
        /// </summary>
        public void ExecuteNextLine()
        {
            if (Finished) return;

            string line = Script[Pointer];

            Context.Core.Module<ScriptEngine>().ExecuteScript(line);

            Pointer++;
        }

    }
}
