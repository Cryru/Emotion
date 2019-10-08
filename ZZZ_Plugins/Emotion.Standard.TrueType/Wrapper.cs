#region Using

using SharpFont;

#endregion

namespace Emotion.Standard.FreeType
{
    public static class Wrapper
    {
        public static void SetCharSize(Face f, float size, int dpi = 96)
        {
            if (dpi == 0)
            {
                f.SetPixelSizes(0, (uint) size);
            }
            else
            {
                f.SetCharSize(size, size, (uint) dpi, (uint) dpi);
            }
        }

        public static GlyphSlot RenderGlyphDefaultOptions(Face f, uint charCode)
        {
            f.LoadChar(charCode, LoadFlags.Render | LoadFlags.ForceAutohint, LoadTarget.Light);
            return f.Glyph;
        }
    }
}