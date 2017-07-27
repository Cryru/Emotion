using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading;
using System.Diagnostics;
using SoulEngine.Modules;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The starting module.
    /// </summary>
    public static class Starter
    {
        #region "Declarations"
        /// <summary>
        /// Define a mutex to check for multiple instances.
        /// </summary>
        static Mutex mutex = new Mutex(true, "{" + Info.GUID + "}");
        /// <summary>
        /// Whether we are loading something.
        /// </summary>
        public static bool Loading = true;
        /// <summary>
        /// A stopwatch used to track boot performance.
        /// </summary>
        public static Stopwatch bootPerformance;
        #endregion

        /// <summary>
        /// The program's entry point.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //Check if an instance of the game is running, and exit if it is.
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                //Start measuring boot performance.
                bootPerformance = Stopwatch.StartNew();

                //Generate the default settings file to be used in case an external one is missing.
                Settings.GenerateDefaultFile();

                //Check if an external settings file exists, and load it's data if it does.
                Settings.ReadExternalSettings("\\settings.soul");

                //Setup a core instance.
                Context.Core = new Core();
                Context.Core.Run();

                // Measure boot time.
                bootPerformance.Stop();
                if (Context.Core.isModuleLoaded<Logger>())
                    Context.Core.Module<Logger>().Add("Engine loading completed in: " + Starter.bootPerformance.ElapsedMilliseconds + "ms");

                //Dispose of the core if it stops running.
                Context.Core.Dispose();
            }
        }
    } 
} 


