using System;
using System.Threading;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev - TheCryru@gmail.com                     //
    //                                                                          //
    // For any questions and issues: https://github.com/Cryru/SoulEngine        //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The engine's start code.
    /// </summary>
    public static class Program
    {

        static Mutex mutex = new Mutex(true, "{" + Core.GUID + "}");

        [STAThread]
        static void Main(string[] args)
        {
            //Check for multi instancing.
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                    //Add suffix to engine.
                    Core.Name += " (OpenGL-Win)";

                    //Run the engine.
                    Core.Setup();
            }
        }
    } 
} 


