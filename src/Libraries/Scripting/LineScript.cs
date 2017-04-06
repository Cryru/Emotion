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
        #endregion

        public LineScript(string[] fileContents, int Pointer)
        {
            this.Pointer = Pointer;
            Script = fileContents;

            if (Pointer != 0)
            {
                for (int i = 0; i < Pointer; i++)
                {
                    if (Script.Length - 1 > i) ScriptEngine.ExecuteScript(Script[i]);
                }
            }
        }

        /// <summary>
        /// Executes the next line.
        /// </summary>
        public void ExecuteNextLine()
        {
            if (Pointer == Script.Length) return;

            string line = Script[Pointer];

            ScriptEngine.ExecuteScript(line);

            Pointer++;
        }

    }
}
