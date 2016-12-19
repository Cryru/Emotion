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
        /// <param name="color">The line's color.</param>
        /// <param name="start">The index to start at.</param>
        /// <param name="end">The index to stop at.</param>
        /// <param name="Thickness">How thick the line should be.</param>
        public static void DrawLine(this SpriteBatch s, Color color, Vector2 start, Vector2 end, int Thickness = 1)
        {
            //Calculate rotation angle.
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            //Draw a stretched blank texture and rotate in the calculated angle.
            s.Draw(SoulEngine.Core.blankTexture.Image, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), Thickness), null, color, angle, new Vector2(0, 0), SpriteEffects.None, 0);
        }
        /// <summary>
        /// Gets a range from within an array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The array.</param>
        /// <param name="index">The starting location, included.</param>
        /// <param name="length">The length of the range, if -1 then until the length is until the end array.</param>
        public static T[] GetRange<T>(this T[] data, int index, int length = -1)
        {
            if (length == -1) length = data.Length - index;

            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
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
