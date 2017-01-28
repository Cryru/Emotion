using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading;
using System.Diagnostics;

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
        #region "Variables"
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

                //Check if an external settings file exists, and load it's data if it does.
                if (System.IO.File.Exists("\\settings.soul")) Settings.ReadExternalSettings("\\settings.soul");

                //Setup a core instance.
                Context.Core = new Core();
                Context.Core.Run();

                //Dispose of the core if it stops running.
                Context.Core.Dispose();
            }
        }

        /// <summary>
        /// Continues the start up process after the engine is set up.
        /// </summary>
        public static void ContinueStart()
        {
            //Check if we are enforcing asset integrity, and check it.
            if (Settings.EnforceAssetIntegrity == true && AssetManager.AssertAssets() == false)
            {
                throw new Exception("The assets meta file is missing, or file tampering detected.");
            }

            //Loading has finished.
            Loading = false;
        }
    } 
} 


