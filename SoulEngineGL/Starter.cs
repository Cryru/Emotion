// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Diagnostics;
using System.Threading;
using Soul.Engine;

#endregion

namespace Soul
{
    /// <summary>
    /// The starting module.
    /// </summary>
    public static class Starter
    {
        #region "Declarations"

        /// <summary>
        /// Define a mutex to check for multiple instances.
        /// </summary>
        private static Mutex _mutex = new Mutex(true, "SoulEngine");

        /// <summary>
        /// Whether we are loading something.
        /// </summary>
        public static bool Loading = true;

        #endregion

        /// <summary>
        /// The program's entry point.
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            //Check if an instance of the game is running, and exit if it is.
            if (_mutex.WaitOne(TimeSpan.Zero, true))
            {
                Core.Setup();

                //Setup a core instance.
                //Context.Core = new Core();
                //Context.Core.Run();

                //// Measure boot time.
                //bootPerformance.Stop();
                //if (Context.Core.isModuleLoaded<Logger>())
                //    Context.Core.Module<Logger>()
                //        .Add("Engine loading completed in: " + bootPerformance.ElapsedMilliseconds + "ms");

                ////Dispose of the core if it stops running.
                //Context.Core.Dispose();
            }
        }
    }
}