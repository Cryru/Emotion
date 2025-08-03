#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Standard.Parsers.OpenType;
using System.Linq;
using static NewStbTrueTypeSharp.StbTrueType;
using OpenGL;
using Emotion.Game.Systems.UI.Text.TextUpdate.LayoutEngineTypes;

namespace Emotion.Game.Systems.UI.Text.TextUpdate;

public class TextRenderEntry
{
    public string SourceString;
    public int Start;
    public int Length;

    public bool Dirty = true;

    public TextRenderEntry(string srcString, int start, int length)
    {
        SourceString = srcString;
        Start = start;
        Length = length;
    }

    public ReadOnlySpan<char> GetString()
    {
        return SourceString.AsSpan(Start, Length);
    }

    public override string ToString()
    {
        return GetString().ToString();
    }
}

public static class TextRenderEngine
{
    public static List<TextRenderEntry> CachedEntries  = new List<TextRenderEntry>(64);
    public static FrameBuffer Atlas;

    public const int ATLAS_TEXTURE_SIZE = 256;
    public static Texture AtlasTexture;
    public static byte[] AtlasTextureCPUSide = new byte[ATLAS_TEXTURE_SIZE * ATLAS_TEXTURE_SIZE]; 

    public static void Init()
    {
        Atlas = new FrameBuffer(new Vector2(4096)).WithColor(true, InternalFormat.R8, OpenGL.PixelFormat.Red);
        AtlasTexture = new Texture();
        AtlasTexture.Upload(new Vector2(ATLAS_TEXTURE_SIZE), null, OpenGL.PixelFormat.Red, InternalFormat.R8);
    }

    public static void CacheEntries(string sourceString, TextBlock textBlock)
    {
        if (Atlas == null) Init(); // todo

        // todo: make sure effects and font match :P
        Span<Range> wordRanges = stackalloc Range[1024];

        ReadOnlySpan<char> str = textBlock.GetBlockString(sourceString);
        int wordCount = str.Split(wordRanges, ' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        for (int i = 0; i < wordCount; i++)
        {
            Range word = wordRanges[i];
            ReadOnlySpan<char> wordStr = str[word];

            // todo
            bool present = false;
            for (int j = 0; j < CachedEntries.Count; j++)
            {
                TextRenderEntry cachedEntry = CachedEntries[j];
                ReadOnlySpan<char> cachedWordStr = cachedEntry.GetString();
                if (wordStr.SequenceEqual(cachedWordStr))
                {
                    present = true;
                    break;
                }
            }

            if (!present)
            {
                (int start, int length) = word.GetOffsetAndLength(str.Length);
                TextRenderEntry entry = new TextRenderEntry(sourceString, start, length);
                CachedEntries.Add(entry);
            }
        }
    }

    public static unsafe void RenderBlock(Renderer c, string sourceString, TextBlock block, Font font)
    {
        for (int x = 0; x < ATLAS_TEXTURE_SIZE; x++)
        {
            for (int y = 0; y < ATLAS_TEXTURE_SIZE; y++)
            {
                AtlasTextureCPUSide[x + y * ATLAS_TEXTURE_SIZE] = 0;
            }
        }


        OtherAsset fontBytes = Engine.AssetLoader.Get<OtherAsset>(FontAsset.DefaultBuiltInFontName, false);
        stbtt_fontinfo stbFont = CreateFont(fontBytes.Content.ToArray(), 0);
        float scaleFactor = stbtt_ScaleForMappingEmToPixels(stbFont, 15);

        stbtt_GetFontVMetrics(stbFont, out int ascent, out int descent, out int lineGap);
        int baseline = (int) (ascent * scaleFactor);

        ReadOnlySpan<char> blockString = block.GetBlockString(sourceString);

        float xpos = 2; // padding
        for (int i = 0; i < blockString.Length; i++)
        {
            char ch = blockString[i];
            if(ch == 'r')
            {
                bool a = true;
            }

            float xShift = xpos - MathF.Floor(xpos);
            stbtt_GetGlyphHMetrics(stbFont, blockString[i], out int advance, out int lsb);

            int x0, y0, x1, y1;
            stbtt_GetCodepointBitmapBoxSubpixel(stbFont, blockString[i], scaleFactor, scaleFactor, xShift, 0, &x0, &y0, &x1, &y1);

            int width = x1 - x0;
            int height = y1 - y0;
            if (width != 0 && height != 0)
            {
                byte[] screen = new byte[width * height];
                fixed (byte* screenPtr = &screen[0])
                {
                    stbtt_MakeCodepointBitmapSubpixel(stbFont, screenPtr, width, height, width, scaleFactor, scaleFactor, xShift, 0, blockString[i]);
                }

                int xAtlas = (int) MathF.Floor(xpos);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int idx = xAtlas + x + (y + baseline + y0) * ATLAS_TEXTURE_SIZE;
                        byte val = AtlasTextureCPUSide[idx];
                        AtlasTextureCPUSide[idx] = (byte) (val | screen[x + y * width]);
                    }
                }
            }

            xpos += advance * scaleFactor;
            if (i < blockString.Length - 1)
            {
                int kerning = stbtt_GetCodepointKernAdvance(stbFont, blockString[i], blockString[i + 1]);
                xpos += scaleFactor * kerning;
            }
        }

        AtlasTexture.Upload(new Vector2(ATLAS_TEXTURE_SIZE), AtlasTextureCPUSide);
        c.RenderSprite(new Vector3(20, 100, 0), new Vector2(ATLAS_TEXTURE_SIZE) * 2f, AtlasTexture);

        //while (text[ch])
        //{
        //    int advance, lsb, x0, y0, x1, y1;
        //    float x_shift = xpos - (float)floor(xpos);
        //    stbtt_GetCodepointHMetrics(&font, text[ch], &advance, &lsb);
        //    stbtt_GetCodepointBitmapBoxSubpixel(&font, text[ch], scale, scale, x_shift, 0, &x0, &y0, &x1, &y1);
        //    stbtt_MakeCodepointBitmapSubpixel(&font, &screen[baseline + y0][(int)xpos + x0], x1 - x0, y1 - y0, 79, scale, scale, x_shift, 0, text[ch]);
        //    // note that this stomps the old data, so where character boxes overlap (e.g. 'lj') it's wrong
        //    // because this API is really for baking character bitmaps into textures. if you want to render
        //    // a sequence of characters, you really need to render each bitmap to a temp buffer, then
        //    // "alpha blend" that into the working buffer
        //    xpos += (advance * scale);
        //    if (text[ch + 1])
        //        xpos += scale * stbtt_GetCodepointKernAdvance(&font, text[ch], text[ch + 1]);
        //    ++ch;
        //}

    }
}
