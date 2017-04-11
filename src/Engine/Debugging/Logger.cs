using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Debugging
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Creates and manages logs on engine processes.
    /// </summary>
    public static class Logger
    {
        #region "Declarations"
        /// <summary>
        /// Whether the logger is enabled.
        /// </summary>
        public static bool Enabled = true;
        /// <summary>
        /// The system log for the current session.
        /// </summary>
        public static List<string> Log
        {
            get
            {
                return log;
            }
        }
        private static List<string> log = new List<string>();
        #endregion

        /// <summary>
        /// Add a message to the logger.
        /// </summary>
        /// <param name="Message"></param>
        public static void Add(string Message)
        {
            if(Enabled) log.Add(Message);
        }
    }
}
