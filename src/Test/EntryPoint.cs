using System;
using System.Threading;
using System.Diagnostics;
using SoulEngine;

namespace Example
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// An entry point for testing.
    /// </summary>
    public static class EntryPoint
    {
        /// <summary>
        /// The program's entry point.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            //Define an instance of the engine and run it.
            Engine engInst = new Engine(new SoulEngine.Modules.Settings() { WindowWidth = 960, WindowHeight = 540, WindowName = "SE18 Test Environment" });
            engInst.Run();
            
            //while(true)
            //{

            //}

        }
    } 
} 


