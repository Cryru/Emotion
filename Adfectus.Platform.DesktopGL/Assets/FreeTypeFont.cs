#region Using

using Adfectus.Graphics.Text;

#endregion

namespace Adfectus.Platform.DesktopGL.Assets
{
    /// <inheritdoc />
    public class FreeTypeFont : Font
    {
        protected override Atlas CreateAtlas(byte[] fontBytes, uint fontSize, int glyphs)
        {
            return new FreeTypeAtlas(fontBytes, fontSize, glyphs);
        }
    }
}