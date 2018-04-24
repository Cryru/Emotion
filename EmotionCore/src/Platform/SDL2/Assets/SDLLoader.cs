// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using System.Collections.Generic;
using System.IO;
using Emotion.Platform.Base;
using Emotion.Platform.Base.Assets;

#endregion

namespace Emotion.Platform.SDL2.Assets
{
    public class SDLLoader : Loader
    {
        public SDLLoader(Context context) : base(context)
        {
        }

        protected override Font LoadFont(byte[] data)
        {
            return new SDLFont(data);
        }

        protected override Texture LoadTexture(byte[] data)
        {
            return new SDLTexture(EmotionContext, data);
        }
    }
}

#endif