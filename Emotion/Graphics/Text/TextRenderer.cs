#nullable enable

using Emotion.Core.Utility.Time;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Shader;
using Emotion.Standard.Memory;
using Emotion.Standard.Parsers.OpenType;
using OpenGL;

namespace Emotion.Graphics.Text;

public static partial class TextRenderer
{
    private static List<TextureAtlasSimple<int>> _atlases = new();

    [ThreadStatic]
    private static NativeArenaAllocator? _arena;

    private static ShaderAsset? _sdfShader;

    private const byte SOLID_PIXEL_THRESHOLD = 85;
    private const bool LETTER_BY_LETTER = false;

    public static int GetAtlasWidth()
    {
        // We divide by two to ensure spacings, SDFs and alignment have a buffer.
        return (int)(TextureAtlasHelper.GetMaxAtlasTextureSize(PixelFormat.Red).X / 2f);
    }

    internal static void Init()
    {
        CreateNewAtlas();
        CreateArenaForThread();
    }

    private static void CreateArenaForThread()
    {
        int mb = 0;
        if (LETTER_BY_LETTER)
            mb = 1;
        else
            mb = 16;

        _arena ??= new((nuint) (1000 * 1000 * mb));
    }

    private static (Texture, Rectangle)? GetRasterizedText(ReadOnlySpan<char> blockString, Font font, int textSize, TextEffect? effect = null)
    {
        // If rendering whole blocks anything but outline blows the CPU budget
        if (effect != null && effect.Value.Type != TextEffectType.Outline)
            return null;

        if (blockString.IsEmpty || blockString.IsWhiteSpace())
            return null;

        // Check if we have it rasterized already
        int stringHash = blockString.GetStableHashCode();
        int fontHash = font.FontHash;
        int effectHash = effect?.GetHashCode() ?? 0;
        int finalHash = HashCode.Combine(stringHash, fontHash, textSize, effectHash);

        foreach (TextureAtlasSimple<int> atlas in _atlases)
        {
            if (atlas.CheckHas(finalHash, out Rectangle uv, true))
                return (atlas.Texture, uv);
        }

        RenderTextOutput output = RasterizeText(blockString, font, textSize, effect);

        // Sanity check
        float atlasSize = GetAtlasWidth();
        if (output.Size.X > atlasSize || output.Size.Y > atlasSize)
        {
            Engine.Log.Warning($"Tried to render text larger than the atlas! '{blockString}' {font.FullName} @ {textSize}", nameof(TextRenderer), true);
            output.Done();
            return null;
        }

        IntVector2 outputResolution = output.Size;
        Span<byte> outputData = output.GetData();

        // Try to add it to each existing atlas
        (Texture, Rectangle)? atlasData = null;
        TextureAtlasSimple<int>? atlasAddedTo = null;
        foreach (TextureAtlasSimple<int> atlas in _atlases)
        {
            Rectangle? uv = atlas.TryAdd(finalHash, outputResolution, outputData);
            if (uv != null)
            {
                atlasAddedTo = atlas;
                atlasData = (atlas.Texture, uv.Value);
                break;
            }
        }

        // No space in any existing atlas - create new atlas
        if (atlasData == null)
        {
            TextureAtlasSimple<int> newAtlas = CreateNewAtlas();
            Rectangle? uv = newAtlas.TryAdd(finalHash, outputResolution, outputData);
            AssertNotNull(uv); // New atlas should always succeed!
            _atlases.Add(newAtlas);

            atlasData = (newAtlas.Texture, uv.Value);
            atlasAddedTo = newAtlas;
        }

        output.Done();
        return atlasData;
    }

    public static void RenderText(
        Renderer r, Vector3 pos, Color color,
        ReadOnlySpan<char> blockString, Font font, int textSize, TextEffect? effect = null
    )
    {
        if (LETTER_BY_LETTER)
        {
            LBL_RenderText(r, pos, color, blockString, font, textSize, effect);
            return;
        }

        if (effect != null)
        {
            TextEffect effectVal = effect.Value;
            Vector3 renderOffset = -effectVal.GetRenderOffset().ToVec3();

            (Texture, Rectangle)? effectRender = TextRenderer.GetRasterizedText(blockString, font, textSize, effect);
            if (effectRender != null)
                r.RenderSprite(pos + renderOffset, effectVal.EffectColor, effectRender.Value.Item1, effectRender.Value.Item2);
        }

        (Texture, Rectangle)? renderData = TextRenderer.GetRasterizedText(blockString, font, textSize);
        if (renderData == null) return; // Can't render this for whatever reason
        r.RenderSprite(pos, color, renderData.Value.Item1, renderData.Value.Item2);
    }

    #region Management

    private static After _atlasCleanupTimer = new After(500);

    internal static void DoTasks()
    {
        // todo: Atlas management in letter by letter mode should have smaller atlases grouped
        // per font or something like that, and only flush on full stale to prevent flicker.
        // Currently it's fine to leave it like this since no one will actually blow the atlas budget with
        // just letters...right?
        if (LETTER_BY_LETTER) return;

        _atlasCleanupTimer.Update(Engine.DeltaTime);
        if (!_atlasCleanupTimer.Finished) return;
        _atlasCleanupTimer.Restart();

        for (int i = 0; i < _atlases.Count; i++)
        {
            TextureAtlasSimple<int> atlas = _atlases[i];
            (int staleItems, int totalItems, float usage) = atlas.TickAllItems(1);
            if (staleItems == 0) continue;

            float staleItemsPercent = (float)staleItems / totalItems;
            bool clearAtlas = staleItemsPercent > 0.5f && usage > 0.5f || staleItemsPercent > 0.75f;
            if (clearAtlas)
            {
                atlas.Clear();
                // Engine.Log.Trace($"Cleared out text atlas (stale items: {staleItemsPercent}; usage: {usage})", nameof(TextRenderSystem));
            }
        }
    }

    #endregion

    #region Helpers

    private static TextureAtlasSimple<int> CreateNewAtlas()
    {
        TextureAtlasSimple<int> newAtlas = new TextureAtlasSimple<int>(PixelFormat.Red, InternalFormat.Red, PixelType.UnsignedByte);

        // Set swizzle so that the texture is read as vec4(1.0, 1.0, 1.0, RED)
        Texture texture = newAtlas.Texture;
        Texture.EnsureBound(texture.Pointer);
        Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureSwizzleR, Gl.ONE);
        Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureSwizzleG, Gl.ONE);
        Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureSwizzleB, Gl.ONE);
        Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureSwizzleA, Gl.RED);
        _atlases.Add(newAtlas);

        return newAtlas;
    }

    #endregion
}
