#nullable enable

using Emotion.Standard.Memory;
using System.Runtime.CompilerServices;

namespace Emotion.Graphics.Text;

public static partial class TextRenderer
{
    private static unsafe void GlyphOutline(NativeArenaAllocator arena, byte* src, byte* dst, int width, int height, int radius)
    {
        int totalPixels = width * height;
        int infSq = (width + height) * (width + height);

        // Horizontal pass
        int* sqDistances = arena.AllocateOfType<int>(totalPixels);
        for (int y = 0; y < height; y++)
        {
            byte* sRow = src + (y * width);
            int* gRow = sqDistances + (y * width);

            int last = -width - height;
            for (int x = 0; x < width; x++)
            {
                if (sRow[x] >= SOLID_PIXEL_THRESHOLD) last = x;
                gRow[x] = (x - last) * (x - last);
            }

            last = width + height;
            for (int x = width - 1; x >= 0; x--)
            {
                if (sRow[x] >= SOLID_PIXEL_THRESHOLD) last = x;
                int d2 = (last - x) * (last - x);
                if (d2 < gRow[x]) gRow[x] = d2;
            }
        }

        // Vertical pass - Meijster's Algorithm
        int* s = stackalloc int[height]; // Indices of parabolas
        int* t = stackalloc int[height]; // Starting points of envelope segments
        for (int x = 0; x < width; x++)
        {
            int q = 0;
            s[0] = 0;
            t[0] = 0;

            // Step A: Construct the lower envelope of parabolas
            for (int u = 1; u < height; u++)
            {
                while (q >= 0 && f(x, t[q], s[q], sqDistances, width) > f(x, t[q], u, sqDistances, width)) q--;

                if (q < 0)
                {
                    q = 0;
                    s[0] = u;
                }
                else
                {
                    int w = 1 + sep(s[q], u, sqDistances[x + s[q] * width], sqDistances[x + u * width]);
                    if (w < height)
                    {
                        q++;
                        s[q] = u;
                        t[q] = w;
                    }
                }
            }

            // Step B: Query the envelope to fill the destination row
            for (int u = height - 1; u >= 0; u--)
            {
                int minDist2 = f(x, u, s[q], sqDistances, width);

                // Alpha calculation
                byte outVal = 0;
                float dist = MathF.Sqrt(minDist2);
                float val = radius + 0.5f - dist;

                if (val >= 1.0f)
                    outVal = 255;
                else if (val > 0.0f)
                    outVal = (byte)(val * 255.0f);

                dst[x + u * width] = outVal;

                if (u == t[q]) q--;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe static int f(int x, int y, int i, int* g, int width)
        => (y - i) * (y - i) + g[x + i * width];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int sep(int i, int j, int gi, int gj)
        => (gj - gi + j * j - i * i) / (2 * (j - i));
}
