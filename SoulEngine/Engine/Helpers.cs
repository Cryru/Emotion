using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Soul.Engine
{
    public static class Helpers
    {
        /// <summary>
        /// Returns the size of a vertices array.
        /// </summary>
        /// <param name="verts">The vertices to calculate.</param>
        /// <param name="scale">The scale multiplier.</param>
        /// <param name="offset">The amount the vertices are offset from 0,0 by.</param>
        /// <returns>A vector2 representing the size of the vertices.</returns>
        public static Vector2 CalculateSizeFromVertices(Vector2[] verts, Vector2 scale, out Vector2 offset)
        {
            // Instance variables.
            offset = new Vector2();
            Vector2 calculatedSize = new Vector2(int.MinValue, int.MinValue);

            for (int i = 0; i < verts?.Length; i++)
            {
                if (verts[i].X * scale.X > calculatedSize.X)
                    calculatedSize.X = (int)(verts[i].X * scale.X);
                else if (verts[i].X * scale.X < offset.X)
                    offset.X = verts[i].X * scale.X;

                if (verts[i].Y * scale.Y > calculatedSize.Y)
                    calculatedSize.Y = (int)(verts[i].Y * scale.Y);
                else if (verts[i].Y * scale.Y < offset.Y)
                    offset.Y = verts[i].Y * scale.Y;
            }

            // Reverse offset.
            offset *= -1;
            // Subtract offset.
            calculatedSize = calculatedSize - offset;

            return calculatedSize;
        }

    }
}
