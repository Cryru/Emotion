using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
        /// Draws a line between two points.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="color"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="Thickness"></param>
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
    /// Extensions to Soul for content checking.
    /// </summary>
    public class IO : Soul.IO
    {
        /// <summary>
        /// Returns whether the content file exists.
        /// </summary>
        /// <param name="name">The name and path of the content file.</param>
        public static bool GetContentExist(string name)
        {
            //Assign the path of the file.
            string contentpath = "Content\\SCon\\" + name.Replace("/", "\\") + ".xnb";
            //Check if the file exists.
            if (File.Exists(contentpath))
            {
                return true;
            }
            return false;
        }
    }
}
