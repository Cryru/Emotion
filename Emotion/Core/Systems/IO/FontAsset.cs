#nullable enable

#region Using

using Emotion.Standard.Parsers.OpenType;

#endregion

namespace Emotion.Core.Systems.IO;

/// <summary>
/// A font file and cached atlas textures.
/// </summary>
public class FontAsset : Asset, IAssetContainingObject<Font>
{
    /// <summary>
    /// The Emotion.Standard.OpenType font generated from the font file.
    /// </summary>
    public Font? Font { get; protected set; }

    /// <inheritdoc />
    protected override void CreateInternal(ReadOnlyMemory<byte> data)
    {
        Font = new Font(data);
    }

    public Font? GetObject()
    {
        return Font != null && Font.Valid ? Font : null;
    }

    /// <inheritdoc />
    protected override void DisposeInternal()
    {

    }

    private static AssetObjectReference<FontAsset, Font> DefaultBuiltInFontName = AssetLoader.NameToEngineName("Editor/UbuntuMono-Regular.ttf");

    /// <summary>
    /// This font is only available if Editor assets are included
    /// and should only be used for prototyping and such.
    /// </summary>
    public static AssetObjectReference<FontAsset, Font> GetDefaultBuiltIn()
    {
        return DefaultBuiltInFontName;
    }
}