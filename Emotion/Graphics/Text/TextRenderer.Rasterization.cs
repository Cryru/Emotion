#nullable enable

using Emotion.Standard.Memory;
using Emotion.Standard.Parsers.OpenType;
using System.Buffers;
using System.Runtime.InteropServices;
using static NewStbTrueTypeSharp.StbTrueType;

namespace Emotion.Graphics.Text;

public static partial class TextRenderer
{
    private class RenderTextOutput
    {
        private static ObjectPoolManual<RenderTextOutput> _pool = new ObjectPoolManual<RenderTextOutput>(
            static () => new RenderTextOutput()
        );

        private byte[]? _data = null;
        public IntVector2 Size;

        public Span<byte> GetData()
        {
            Assert(_data != null);

            int dataLength = Size.X * Size.Y;
            Span<byte> dataSpan = _data.AsSpan();
            return dataSpan.Slice(0, dataLength);
        }

        public static RenderTextOutput Get(int w, int h)
        {
            int dataLength = w * h;

            RenderTextOutput poolItem = _pool.Get();
            if (poolItem._data == null || poolItem._data.Length < dataLength)
            {
                if (poolItem._data != null)
                    ArrayPool<byte>.Shared.Return(poolItem._data);
                poolItem._data = ArrayPool<byte>.Shared.Rent(dataLength);
            }

            Array.Clear(poolItem._data, 0, dataLength);

            poolItem.Size = new IntVector2(w, h);
            return poolItem;
        }

        public void Done()
        {
            _pool.Return(this);
        }
    }

    // GPU wants single component textures aligned to 4
    private const float ALIGN = 4.0f;
    private const int ALIGN_INT = (int)ALIGN;

    private static unsafe RenderTextOutput RasterizeText(ReadOnlySpan<char> text, Font font, int textSize, TextEffect? effect, float forcexShift = 0)
    {
        CreateArenaForThread();
        AssertNotNull(_arena);
        _arena.Clear();

        float scale = font.GetScaleFromFontSize(textSize);

        stbtt_fontinfo stbFont = font.StbFontInfo;
        stbtt_GetFontVMetrics(stbFont, out int ascent, out int descent, out int lineGap);
        int baseline = (int)(ascent * scale);

        int TEMP_ATLAS_HEIGHT = (int)MathF.Ceiling((ascent - descent) * scale);
        int TEMP_ATLAS_WIDTH = (int)MathF.Ceiling(text.Length * textSize);
        int tempAtlasSize = TEMP_ATLAS_WIDTH * TEMP_ATLAS_HEIGHT;

        byte* tempAtlas = (byte*)_arena.Allocate((nuint)tempAtlasSize);
        NativeMemory.Clear(tempAtlas, (nuint)tempAtlasSize);

        int maxCharScreenSize = (textSize * textSize) * 2;
        byte* charScreen = (byte*) _arena.Allocate((nuint) maxCharScreenSize);

        float xPos = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];

            int glyphIdx = font.GetGlyphIndexFromChar(ch);
            stbtt_GetGlyphHMetrics(stbFont, glyphIdx, out int advance, out int lsb);

            float xShift = xPos - MathF.Floor(xPos);
            if (forcexShift != 0) xShift = forcexShift;

            int x0, y0, x1, y1;
            stbtt_GetGlyphBitmapBoxSubpixel(stbFont, glyphIdx, scale, scale, xShift, 0, &x0, &y0, &x1, &y1);

            int width = x1 - x0;
            int height = y1 - y0;

            if (width * height > maxCharScreenSize)
            {
                Engine.Log.Error($"Character {ch} at size {textSize} from font {font.FullName} was too large to render :/", "TextRender");
            }
            else if (width != 0 && height != 0)
            {
                stbtt_MakeGlyphBitmapSubpixel(_arena, stbFont, charScreen, x0, y0, width, height, width, scale, scale, xShift, 0, glyphIdx);

                // Copy to temp atlas
                int xAtlas = (int)MathF.Floor(xPos + x0);
                xAtlas = Math.Max(xAtlas, 0); // Sometimes first letters have a negative X (todo: investigate)

                int yAtlas = baseline + y0;
                yAtlas = Math.Max(yAtlas, 0);

                for (int y = 0; y < height; y++)
                {
                    int destY = yAtlas + y;
                    for (int x = 0; x < width; x++)
                    {
                        int destX = xAtlas + x;
                        int idx = destX + destY * TEMP_ATLAS_WIDTH;
                        tempAtlas[idx] |= charScreen[x + y * width];
                    }
                }
            }

            xPos += advance * scale;
            if (i < text.Length - 1)
            {
                int nextGlyphIndex = font.GetGlyphIndexFromChar(text[i + 1]);
                int kerning = stbtt_GetCodepointKernAdvance(stbFont, glyphIdx, nextGlyphIndex);
                xPos += kerning * scale;
            }
        }

        // Copy the temp atlas to an output atlas (since we cant upload with a custom stride)
        IntVector2 filledSize = IntVector2.FromVec2Ceiling(new Vector2(xPos, TEMP_ATLAS_HEIGHT));

        IntVector2 outputSize = filledSize;
        int outputOffsetX = 0;
        int outputOffsetY = 0;

        TextEffect effectVal = effect.GetValueOrDefault();
        bool hasEffect = effectVal.EffectValue != 0;
        if (hasEffect)
        {
            outputSize += effectVal.GrowAtlas();

            IntVector2 offset = effectVal.GetRenderOffset();
            outputOffsetX += offset.X;
            outputOffsetY += offset.Y;
        }

        outputSize.X = (int)MathF.Ceiling(outputSize.X / ALIGN) * ALIGN_INT;
        outputSize.Y = (int)MathF.Ceiling(outputSize.Y / ALIGN) * ALIGN_INT;

        RenderTextOutput output = RenderTextOutput.Get(outputSize.X, outputSize.Y);
        Span<byte> outputData = output.GetData();
        for (int y = 0; y < filledSize.Y; y++)
        {
            int dstY = (y + outputOffsetY) * outputSize.X;
            int srcY = y * TEMP_ATLAS_WIDTH;
            for (int x = 0; x < filledSize.X; x++)
            {
                outputData[x + outputOffsetX + dstY] = tempAtlas[x + srcY];
            }
        }

        if (hasEffect)
        {
            _arena.Clear();

            int effectStrength = effectVal.EffectValue;
            if (effectVal.Type == TextEffectType.Outline)
            {
                byte* src = _arena.AllocateOfType<byte>(outputData.Length);
                fixed (byte* dst = &outputData[0])
                {
                    NativeMemory.Copy(dst, src, (nuint)outputData.Length);
                    GlyphOutline(_arena, src, dst, outputSize.X, outputSize.Y, effectStrength);
                }
            }
            else if (effectVal.Type == TextEffectType.Shadow)
            {
                Span<byte> outputOriginal = _arena.AllocateSpan<byte>(outputData.Length);
                outputData.CopyTo(outputOriginal);
                GlyphShadow(_arena, outputOriginal, outputData, outputSize.X, outputSize.Y, effectStrength);
            }
        }

        return output;
    }

    public static unsafe void GlyphShadow(NativeArenaAllocator arena, ReadOnlySpan<byte> src, Span<byte> dst, int width, int height, int offset)
    {
        int blurRadius = offset;
        int offsetX = offset;
        int offsetY = (int)Math.Ceiling(offset * 0.75f);

        // Temporary buffer for horizontal pass
        Span<float> temp = arena.AllocateSpan<float>(width * height);
        temp.Clear();

        // Precompute Gaussian kernel
        int kernelRadius = blurRadius;
        float sigma = blurRadius / 2.0f;
        Span<float> kernel = arena.AllocateSpan<float>(kernelRadius * 2 + 1);
        kernel.Clear();

        float sum = 0.0f;

        for (int i = -kernelRadius; i <= kernelRadius; i++)
        {
            float v = MathF.Exp(-(i * i) / (2.0f * sigma * sigma));
            kernel[i + kernelRadius] = v;
            sum += v;
        }

        // Normalize
        for (int i = 0; i < kernel.Length; i++)
            kernel[i] /= sum;

        // --- Horizontal pass ---
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float accum = 0.0f;

                int sx = x - offsetX;
                int sy = y - offsetY;
                if (sy < 0) continue;

                for (int k = -kernelRadius; k <= kernelRadius; k++)
                {
                    int nx = sx + k;
                    if ((uint)nx >= width) continue;

                    byte a = src[nx + sy * width];
                    if (a < SOLID_PIXEL_THRESHOLD) continue;

                    accum += a * kernel[k + kernelRadius];
                }

                temp[x + y * width] = accum;
            }
        }

        // --- Vertical pass ---
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float accum = 0.0f;

                for (int k = -kernelRadius; k <= kernelRadius; k++)
                {
                    int ny = y - offsetY + k;
                    if ((uint)ny >= height) continue;

                    accum += temp[x + ny * width] * kernel[k + kernelRadius];
                }

                // Clamp and subtract original glyph
                int idx = x + y * width;
                byte val = (byte)Math.Clamp((int)accum, 0, 255);
                dst[idx] = (byte)Math.Max(val - src[idx], 0);
            }
        }
    }
}
