using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Modules;
using SoulEngine.Libraries;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The SoulEngine context object.
    /// </summary>
    public class Engine
    {
        #region Declarations
        /// <summary>
        /// The engine's context.
        /// </summary>
        public Context Context;
        #endregion

        #region Boot Process
        /// <summary>
        /// Create a new engine instance.
        /// </summary>
        /// <param name="Settings">The settings to run the engine with.</param>
        public Engine(Settings Settings)
        {
            //Initialize Context.
            Context = new Context();

            //Assign settings object.
            Context.Settings = Settings;
        }
        /// <summary>
        /// Starts the engine.
        /// </summary>
        public void Run()
        {
            Raya.Initialize(true, 3, 0);

            Raya.CreateWindow(
                Context.Settings.WindowName,
                Context.Settings.WindowWidth,
                Context.Settings.WindowHeight
                );
            
            while (true)
            {
                Raya.Tick();

            }


        }
        

        #endregion


    }
}
