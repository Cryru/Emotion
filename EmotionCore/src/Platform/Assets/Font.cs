// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using System.Collections.Generic;
using System.IO;
using Emotion.Platform;
using FreeImageAPI;
using SDL2;

#endregion

namespace Emotion.Platform.Assets
{
    public class Font
    {
        /// <summary>
        /// A pointer to font's stream bytes.
        /// </summary>
        internal IntPtr StreamPointer;

        /// <summary>
        /// A collection of pointers of various texture sizes of the font.
        /// </summary>
        private Dictionary<int, IntPtr> _sizePointers;

        public Font(byte[] fontBytes)
        {
            _sizePointers = new Dictionary<int, IntPtr>();

            // Convert to an SDL stream.
            StreamPointer = SDL.SDL_RWFromMem(fontBytes, fontBytes.Length);
        }

        public IntPtr GetSize(int size)
        {
            // Check if the requested size is already loaded.
            if (_sizePointers.ContainsKey(size)) return _sizePointers[size];

            // Load into SDL.
            IntPtr sizePointer = ErrorHandler.CheckError(SDLTtf.TTF_OpenFontRW(StreamPointer, 0, size));
            _sizePointers.Add(size, sizePointer);
            return sizePointer;
        }
    }
}

#endif