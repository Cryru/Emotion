using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Modules
{
    class ErrorManager : IModule
    {
        public bool Initialize()
        {
            return true;
        }

        /// <summary>
        /// Raises an error.
        /// </summary>
        /// <param name="Error">The error message.</param>
        /// <param name="Severity">How severe the error is 0-255. Can also be used as an error code.</param>
        public void RaiseError(string Error, byte Severity)
        {
            if(Context.Core.isModuleLoaded<Logger>())
                Context.Core.Module<Logger>().Add("Error [" + Severity + "] " + Error);

            if(Severity > Settings.ErrorSupressionLevel)
            {
                Context.Core.Exit();
                throw new Exception("Error [" + Severity + "] " + Error);
            }
        }

        // Error Codes:
        // 3 - Invalid color argument.
        // 50 - Javascript execution error.
        // 100 - Module failed to load.
        // 101 - Expected module wasn't loaded.
        // 180 - Scene load called directly.
        // 240 - Global assets failed to load.
        // 241 - meta.soul - Missing.
        // 242 - meta.soul - Wrong format.
        // 243 - meta.soul - Hash doesn't match expected.
        // 244 - Asset validation failed.
        // 244 - Failed to validate file.
    }
}
