// Emotion - https://github.com/Cryru/Emotion

using System;

#if SDL2

namespace Emotion.Platform.SDL2.Base
{
    /// <summary>
    /// A native SDL2 object.
    /// </summary>
    internal interface INativeObject
    {
        /// <summary>
        /// The pointer to the native SDL2 object.
        /// </summary>
        IntPtr Pointer { get; set; }
    }
}


#endif