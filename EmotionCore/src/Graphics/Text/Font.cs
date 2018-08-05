// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.IO;

#endregion

namespace Emotion.Graphics.Text
{
    public sealed class Font : Asset
    {
        public Atlas Atlas { get; private set; }

        internal override void Create(byte[] data)
        {
            Atlas = new Atlas(data, 12);

        }

        internal override void Destroy()
        {
            Atlas.Texture.Destroy();
        }
    }
}