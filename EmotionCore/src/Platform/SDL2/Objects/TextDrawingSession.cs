// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using Emotion.Platform.SDL2.Assets;

#endregion

namespace Emotion.Platform.SDL2.Objects
{
    /// <summary>
    /// A text drawing session. Used as an object holder by the Renderer when rendering text.
    /// </summary>
    public class TextDrawingSession
    {
        internal IntPtr Font;
        internal IntPtr Surface;
        internal SDLTexture Cache;

        internal int FontAscent;
        internal int YOffset;
        internal int XOffset;

        public bool Finalized { get; internal set; }
    }
}

#endif