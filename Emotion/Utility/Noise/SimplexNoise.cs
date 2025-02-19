#nullable enable

namespace Emotion.Utility.Noise;

public class SimplexNoise
{
    private readonly int[] _permutation;

    private static readonly Vector3[] _grad3 =
    {
        new Vector3(1, 1, 0), new Vector3(-1, 1, 0), new Vector3(1, -1, 0), new Vector3(-1, -1, 0),
        new Vector3(1, 0, 1), new Vector3(-1, 0, 1), new Vector3(1, 0, -1), new Vector3(-1, 0, -1),
        new Vector3(0, 1, 1), new Vector3(0, -1, 1), new Vector3(0, 1, -1), new Vector3(0, -1, -1)
    };

    public SimplexNoise(Random? rng = null)
    {
        rng ??= new Random();

        _permutation = new int[512];
        int[] p = new int[256];
        for (int i = 0; i < 256; i++)
            p[i] = i;

        for (int i = 255; i > 0; i--)
        {
            int swapIndex = rng.Next(i + 1);
            int temp = p[i];
            p[i] = p[swapIndex];
            p[swapIndex] = temp;
        }

        for (int i = 0; i < 512; i++)
            _permutation[i] = p[i & 255];
    }

    public float Sample2D(Vector2 pos)
    {
        float x = pos.X;
        float y = pos.Y;

        float F2 = 0.5f * (float)(Math.Sqrt(3.0) - 1.0);
        float G2 = (3.0f - (float)Math.Sqrt(3.0)) / 6.0f;

        float s = (x + y) * F2;
        int i = (int) Math.Floor(x + s);
        int j = (int) Math.Floor(y + s);
        float t = (i + j) * G2;
        float X0 = i - t;
        float Y0 = j - t;
        float x0 = x - X0;
        float y0 = y - Y0;

        int i1, j1;
        if (x0 > y0) { i1 = 1; j1 = 0; } else { i1 = 0; j1 = 1; }

        float x1 = x0 - i1 + G2;
        float y1 = y0 - j1 + G2;
        float x2 = x0 - 1.0f + 2.0f * G2;
        float y2 = y0 - 1.0f + 2.0f * G2;

        int ii = i & 255;
        int jj = j & 255;
        int gi0 = _permutation[ii + _permutation[jj]] % 12;
        int gi1 = _permutation[ii + i1 + _permutation[jj + j1]] % 12;
        int gi2 = _permutation[ii + 1 + _permutation[jj + 1]] % 12;

        float t0 = 0.5f - x0 * x0 - y0 * y0;
        float n0 = t0 < 0 ? 0.0f : (float)Math.Pow(t0, 4) * Vector2.Dot(new Vector2(_grad3[gi0].X, _grad3[gi0].Y), new Vector2(x0, y0));

        float t1 = 0.5f - x1 * x1 - y1 * y1;
        float n1 = t1 < 0 ? 0.0f : (float)Math.Pow(t1, 4) * Vector2.Dot(new Vector2(_grad3[gi1].X, _grad3[gi1].Y), new Vector2(x1, y1));

        float t2 = 0.5f - x2 * x2 - y2 * y2;
        float n2 = t2 < 0 ? 0.0f : (float)Math.Pow(t2, 4) * Vector2.Dot(new Vector2(_grad3[gi2].X, _grad3[gi2].Y), new Vector2(x2, y2));

        return 70.0f * (n0 + n1 + n2);

        return 0;
    }
}
