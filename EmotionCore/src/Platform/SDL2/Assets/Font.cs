// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using System.Collections.Generic;
using Emotion.Primitives;
using SDL2;

#endregion

namespace Emotion.Platform.SDL2.Assets
{
    public class Font
    {
        /// <summary>
        /// A pointer to font's stream bytes.
        /// </summary>
        internal byte[] FontBytes;

        /// <summary>
        /// A collection of pointers of various texture sizes of the font.
        /// </summary>
        private Dictionary<int, IntPtr> _sizePointers;

        public Font(byte[] fontBytes)
        {
            _sizePointers = new Dictionary<int, IntPtr>();

            FontBytes = fontBytes;
        }

        public IntPtr GetSize(int size)
        {
            // Check if the requested size is already loaded.
            if (_sizePointers.ContainsKey(size)) return _sizePointers[size];

            // Load into SDL the font at the requested size.

            // Convert to an SDL stream.
            IntPtr streamPointer = SDL.SDL_RWFromConstMem(FontBytes, FontBytes.Length);

            // Load at the requested size.
            IntPtr sizePointer = ErrorHandler.CheckError(SDLTtf.TTF_OpenFontRW(streamPointer, 1, size));
            _sizePointers.Add(size, sizePointer);
            return sizePointer;
        }

        public Vector2 MeasureString(string text, int size)
        {
            IntPtr fontPointer = GetSize(size);
            SDLTtf.TTF_SizeText(fontPointer, text, out int w, out int h);

            return new Vector2(w, h);
        }

        public int LineSpacing(int size)
        {
            IntPtr fontPointer = GetSize(size);
            return SDLTtf.TTF_FontLineSkip(fontPointer);
        }
    }
}

#endif