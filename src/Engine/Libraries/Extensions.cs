using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev - TheCryru@gmail.com                     //
    //                                                                          //
    // For any questions and issues: https://github.com/Cryru/SoulEngine        //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Extensions to other classes.
    /// </summary>
    static class Extensions
    {
        /// <summary>
        /// Converts a XNA Vector2 to a Physics Engine Vector2.
        /// </summary>
        public static SoulEngine.Physics.Vector2 ToPhys(this Vector2 t)
        {
            return new SoulEngine.Physics.Vector2(t.X, t.Y);
        }

        /// <summary>
        /// Converts a Physics Engine Vector2 to a XNA Vector2.
        /// </summary>
        public static Vector2 ToNorm(this SoulEngine.Physics.Vector2 t)
        {
            return new Vector2(t.X, t.Y);
        }
    }
}
