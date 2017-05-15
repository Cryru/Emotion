using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    // Using Raya: https://github.com/Cryru/Raya                                //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A C# wrapper for Raya.
    /// </summary>
    public static class Raya
    {
        [DllImport("Raya.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Initialize(string Name, int WindowWidth, int WindowHeight);
    }
}
