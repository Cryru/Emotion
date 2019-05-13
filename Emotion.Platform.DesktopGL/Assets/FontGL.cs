#region Using

using Adfectus.Graphics.Text;

#endregion

namespace Emotion.Platform.DesktopGL.Assets
{
    /// <inheritdoc />
    public class FontGL : Font
    {
        protected override Atlas CreateAtlas(byte[] fontBytes, uint fontSize, int glyphs)
        {
            return new AtlasGL(fontBytes, fontSize, glyphs);
        }
    }
}