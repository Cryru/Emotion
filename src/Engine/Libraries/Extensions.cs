using Microsoft.Xna.Framework.Graphics;
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

        public static void DrawLine(this SpriteBatch s, Color color, Vector2 start, Vector2 end, int Thickness = 1)
        {
            //Calculate rotation angle.
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            //Draw a stretched blank texture and rotate in the calculated angle.
            s.Draw(SoulEngine.Core.blankTexture.Image, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), Thickness), null, color, angle, new Vector2(0, 0), SpriteEffects.None, 0);
        }
    }
}
