// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.IO;
using ftgl;

#endregion

namespace Emotion.Graphics.Text
{
    public sealed class Font : Asset
    {
        public texture_font_t FtglFont;
        public texture_atlas_t FtglAtlas;

        internal override void Create(byte[] data)
        {
            FtglAtlas = FreeTypeGL.texture_atlas_new(512, 512, 1);
            FtglFont = FreeTypeGL.texture_font_new_from_memory(FtglAtlas, 12, data);
            FreeTypeGL.texture_font_get_glyph(FtglFont, (int) 'A');
        }

        internal override void Destroy()
        {
            FreeTypeGL.texture_atlas_delete(FtglAtlas);
            FreeTypeGL.texture_font_delete(FtglFont);
        }
    }
}