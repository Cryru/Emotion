#nullable enable

#region Using

using Emotion.Core.Systems.IO;
using Emotion.Graphics.Assets;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.Text;
using Emotion.Standard.Parsers.OpenType;

#endregion

namespace Emotion.Graphics;

public sealed partial class Renderer
{
    private TextLayouter _globalTextLayouter = new TextLayouter(false);

    /// <inheritdoc cref="RenderString(Vector3, Color, ReadOnlySpan{char}, AssetObjectReference{FontAsset, Font}, int)" />
    public void RenderString(Vector3 position, Color color, string text, AssetObjectReference<FontAsset, Font> font, int fontSize)
    {
        RenderString(position, color, text.AsSpan(), font, fontSize);
    }

    /// <inheritdoc cref="RenderString(Vector3, Color, ReadOnlySpan{char}, AssetObjectReference{FontAsset, Font}, int)" />
    public void RenderString(Vector3 position, ReadOnlySpan<char> text, int fontSize)
    {
        RenderString(position, Color.White, text, fontSize);
    }

    /// <inheritdoc cref="RenderString(Vector3, Color, ReadOnlySpan{char}, AssetObjectReference{FontAsset, Font}, int)" />
    public void RenderString(Vector3 position, Color color, ReadOnlySpan<char> text, int fontSize)
    {
        RenderString(position, color, text, FontAsset.GetDefaultBuiltIn(), fontSize);
    }

    /// <summary>
    /// Render a string at the specified position.
    /// </summary>
    public void RenderString(Vector3 position, Color color, ReadOnlySpan<char> text, AssetObjectReference<FontAsset, Font> font, int fontSize)
    {
        _globalTextLayouter.RunLayout(text, fontSize, font.GetObjectLoadinline(), null, GlyphHeightMeasurement.FullHeight);
        _globalTextLayouter.RenderLastLayout(this, position, color);
    }
}