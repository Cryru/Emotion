using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.Graphics.Text
{
    public struct Glyph
    {
        /// <summary>
        /// Coordinates within the texture atlas.
        /// </summary>
        
        public int X;
        public int Y;
        public int Width;
        public int Height;

        /// <summary>
        /// Offsets for renderings.
        /// </summary>

        public int XOffset;
        public int YOffset;
        public int Advance;
    }
}
