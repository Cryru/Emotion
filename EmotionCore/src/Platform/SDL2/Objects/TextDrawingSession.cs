using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emotion.Platform.SDL2.Assets;

namespace Emotion.Platform.SDL2.Objects
{
    public class TextDrawingSession
    {
        public IntPtr Font;
        public IntPtr Surface;
        public Texture Cache;

        public int FontAscent;
        public int YOffset;
        public int XOffset;
    }
}
