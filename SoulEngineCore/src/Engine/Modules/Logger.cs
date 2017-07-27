using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Modules
{
    public class Logger : IModule
    {
        private List<string> Log;

        public bool Initialize()
        {
            // If logging is enabled then initialize the log list and hook to the closing event.
            if (Settings.Log)
            {
                Log = new List<string>();
                Context.Core.Exiting += Core_Exiting;
            }

            return true;
        }

        private void Core_Exiting(object sender, EventArgs e)
        {
            // If the engine is closing dump the log.
            DumpLog();
        }

        /// <summary>
        /// Add a message to the log.
        /// </summary>
        /// <param name="Message">The message to add.</param>
        public void Add(string Message)
        {
            if(Log != null)
            {
                Log.Add(Message);
            }

            // Check if time to dump log.
            if(Log.Count > 5000) DumpLog();
        }

        /// <summary>
        /// Saves the log to a file.
        /// </summary>
        private void DumpLog()
        {
            // Check if logging.
            if (Log == null) return;
            
            // Create logs folder if it doesn't exist and save log.
            Soul.IO.Utils.CreateDirectory("Logs");
            Soul.IO.Utils.WriteFile("Logs" + Path.DirectorySeparatorChar + DateTime.Now.ToFileTime() + ".log", GetLog());
        }

        /// <summary>
        /// Returns the log formatted for printing.
        /// </summary>
        /// <returns></returns>
        public string GetLog()
        {
            return string.Join("\r\n", Log);
        }
    }
}
