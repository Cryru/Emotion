#region Using

using System.Linq;
using Emotion.Standard.Parsers.OpenType;

#endregion

#nullable enable

namespace Emotion.Graphics.Text.StbRenderer
{
    public static class StbGlyphRendererBackend
    {
        public static void RenderGlyph(Font font, StbGlyphCanvas canvas, DrawableGlyph glyph, float scale, bool invertY = false)
        {
            if (scale == 0f) return;
            if (canvas.Width == 0 || canvas.Height == 0) return;

            const float flatnessInPixels = 0.35f;
            Vector2[]? windings = FlattenCurves(glyph.FontGlyph.Commands, flatnessInPixels / scale, out int[]? contourLengths);

            if (windings == null || contourLengths == null) return;

            float scaleX = scale;
            float scaleY = scale;
            if (invertY) scaleY = -scaleY;
            int n = contourLengths.Sum();

            var e = new StbRendererGlyphEdge[n + 1];

            n = 0;
            var wOffset = 0;
            var subSample = 1;
            for (var i = 0; i < contourLengths.Length; i++)
            {
                int j = contourLengths[i] - 1;
                for (var k = 0; k < contourLengths[i]; j = k++)
                {
                    int a = k;
                    int b = j;
                    if (windings[wOffset + j].Y == windings[wOffset + k].Y)
                        continue;
                    e[n].Invert = false;
                    if ((invertY && windings[wOffset + j].Y > windings[wOffset + k].Y) || (!invertY && windings[wOffset + j].Y < windings[wOffset + k].Y))
                    {
                        e[n].Invert = true;
                        a = j;
                        b = k;
                    }

                    e[n].X = windings[wOffset + a].X * scaleX;
                    e[n].Y = windings[wOffset + a].Y * scaleY * subSample;
                    e[n].X1 = windings[wOffset + b].X * scaleX;
                    e[n].Y1 = windings[wOffset + b].Y * scaleY * subSample;
                    n++;
                }

                wOffset += contourLengths[i];
            }

            Array.Resize(ref e, n + 1); // todo

            QuickSortEdges(new Span<StbRendererGlyphEdge>(e), n);

            // Insert Sort
            for (var i = 1; i < n; i++)
            {
                StbRendererGlyphEdge t = e[i];
                int j = i;
                while (j > 0)
                {
                    ref StbRendererGlyphEdge b = ref e[j - 1];
                    if (!(t.Y < b.Y))
                        break;
                    e[j] = e[j - 1];
                    j--;
                }

                if (i != j)
                    e[j] = t;
            }

            var bbox = new Rectangle(MathF.Floor(glyph.XBearing), MathF.Round(font.Descender * scale), canvas.Width, canvas.Height);
            StbRendererActiveEdge? active = null;
            {
                Span<float> scanlineData = new float[129];
                Span<float> scanline = canvas.Width > 64 ? new float[canvas.Width * 2 + 1] : scanlineData;
                Span<float> scanline2 = scanline.Slice(canvas.Width);

                var edgeIndex = 0;

                var y = (int) bbox.Y;
                e[n].Y = bbox.Y + canvas.Height + 1;

                var j = 0;
                while (j < canvas.Height)
                {
                    float scanYTop = y + 0.0f;
                    float scanYBottom = y + 1.0f;

                    for (var i = 0; i < canvas.Width + 1; i++)
                    {
                        scanline2[i] = 0;
                        if (i == canvas.Width) break;
                        scanline[i] = 0;
                    }

                    StbRendererActiveEdge? step = active;

                    while (step != null)
                    {
                        StbRendererActiveEdge z = step;
                        if (z.Ey <= scanYTop)
                        {
                            step = z.Next;
                            z.Direction = 0;
                        }
                        else
                        {
                            step = step.Next;
                        }
                    }

                    while (e[edgeIndex].Y <= scanYBottom)
                    {
                        if (e[edgeIndex].Y != e[edgeIndex].Y1)
                        {
                            var z = new StbRendererActiveEdge(e[edgeIndex], (int) bbox.X, scanYTop);
                            if (j == 0 && bbox.Y != 0)
                                if (z.Ey < scanYTop)
                                    z.Ey = scanYTop;

                            z.Next = active;
                            active = z;
                        }

                        edgeIndex++;
                    }

                    if (active != null)
                        FillActiveEdge(scanline, scanline2, canvas.Width, active, scanYTop);

                    // Transfer scanlines.
                    float sum = 0;
                    for (var i = 0; i < canvas.Width; ++i)
                    {
                        sum += scanline2[i];
                        float k = scanline[i] + sum;
                        k = (float) Math.Abs((double) k) * 255 + 0.5f;
                        var pixel = (int) k;
                        // Clamp down.
                        if (pixel > 255)
                            pixel = 255;

                        // Write to canvas.
                        canvas.Data[j * canvas.Width + i] = (byte) pixel;
                    }

                    step = active;
                    while (step != null)
                    {
                        StbRendererActiveEdge z = step;
                        z.Fx += z.Fdx;
                        step = step.Next;
                    }

                    ++y;
                    ++j;
                }
            }
        }

        public static void HandleClippedEdge(Span<float> scanline, int x, StbRendererActiveEdge e, float x0, float y0, float x1, float y1)
        {
            if (y0 == y1)
                return;

            Assert(y0 < y1);
            Assert(e.Sy <= e.Ey);

            if (y0 > e.Ey)
                return;
            if (y1 < e.Sy)
                return;
            if (y0 < e.Sy)
            {
                x0 += (x1 - x0) * (e.Sy - y0) / (y1 - y0);
                y0 = e.Sy;
            }

            if (y1 > e.Ey)
            {
                x1 += (x1 - x0) * (e.Ey - y1) / (y1 - y0);
                y1 = e.Ey;
            }

            if (x0 == x)
                Assert(x1 <= x + 1);
            else if (x0 == x + 1)
                Assert(x1 >= x);
            else if (x0 <= x)
                Assert(x1 <= x);
            else if (x0 >= x + 1)
                Assert(x1 >= x + 1);
            else
                Assert(x1 >= x && x1 <= x + 1);

            if (x0 <= x && x1 <= x)
            {
                scanline[x] += e.Direction * (y1 - y0);
            }
            else if (!(x0 >= x + 1 && x1 >= x + 1))
            {
                Assert(x0 >= x && x0 <= x + 1 && x1 >= x && x1 <= x + 1);
                scanline[x] += e.Direction * (y1 - y0) * (1 - (x0 - x + (x1 - x)) / 2);
            }
        }

        private static void FillActiveEdge(Span<float> scanline, Span<float> scanlineFill, int length, StbRendererActiveEdge e, float yTop)
        {
            float yBottom = yTop + 1;
            while (e != null)
            {
                if (e.Fdx == 0)
                {
                    float x0 = e.Fx;
                    if (x0 < length)
                    {
                        if (x0 >= 0)
                        {
                            HandleClippedEdge(scanline, (int) x0, e, x0, yTop,
                                x0, yBottom);
                            HandleClippedEdge(scanlineFill, (int) x0 + 1, e, x0,
                                yTop, x0, yBottom);
                        }
                        else
                        {
                            HandleClippedEdge(scanlineFill, 0, e, x0, yTop,
                                x0, yBottom);
                        }
                    }
                }
                else
                {
                    float x0 = e.Fx;
                    float dx = e.Fdx;
                    float xb = x0 + dx;
                    float dy = e.Fdy;
                    float sy0;
                    float xTop;
                    if (e.Sy > yTop)
                    {
                        xTop = x0 + dx * (e.Sy - yTop);
                        sy0 = e.Sy;
                    }
                    else
                    {
                        xTop = x0;
                        sy0 = yTop;
                    }

                    float sy1;
                    float xBottom;
                    if (e.Ey < yBottom)
                    {
                        xBottom = x0 + dx * (e.Ey - yTop);
                        sy1 = e.Ey;
                    }
                    else
                    {
                        xBottom = xb;
                        sy1 = yBottom;
                    }

                    if (xTop >= 0 && xBottom >= 0 && xTop < length && xBottom < length)
                    {
                        if ((int) xTop == (int) xBottom)
                        {
                            var x = (int) xTop;
                            float height = sy1 - sy0;
                            scanline[x] += e.Direction * (1 - (xTop - x + (xBottom - x)) / 2) * height;
                            scanlineFill[1 + x] += e.Direction * height;
                        }
                        else
                        {
                            if (xTop > xBottom)
                            {
                                sy0 = yBottom - (sy0 - yTop);
                                sy1 = yBottom - (sy1 - yTop);
                                float t = sy0;
                                sy0 = sy1;
                                sy1 = t;
                                t = xBottom;
                                xBottom = xTop;
                                xTop = t;
                                dy = -dy;
                                x0 = xb;
                            }

                            var x1 = (int) xTop;
                            var x2 = (int) xBottom;
                            float yCrossing = (x1 + 1 - x0) * dy + yTop;
                            float sign = e.Direction;
                            float area = sign * (yCrossing - sy0);
                            scanline[x1] += area * (1 - (xTop - x1 + (x1 + 1 - x1)) / 2);
                            float step = sign * dy;
                            int x;
                            for (x = x1 + 1; x < x2; ++x)
                            {
                                scanline[x] += area + step / 2;
                                area += step;
                            }

                            yCrossing += dy * (x2 - (x1 + 1));
                            scanline[x2] +=
                                area + sign * (1 - (x2 - x2 + (xBottom - x2)) / 2) * (sy1 - yCrossing);
                            scanlineFill[1 + x2] += sign * (sy1 - sy0);
                        }
                    }
                    else
                    {
                        int x;
                        for (x = 0; x < length; ++x)
                        {
                            float y0 = yTop;
                            float x1 = x;
                            float x2 = x + 1;
                            float x3 = xb;
                            float y3 = yBottom;
                            float y1 = (x - x0) / dx + yTop;
                            float y2 = (x + 1 - x0) / dx + yTop;
                            if (x0 < x1 && x3 > x2)
                            {
                                HandleClippedEdge(scanline, x, e, x0, y0,
                                    x1, y1);
                                HandleClippedEdge(scanline, x, e, x1, y1,
                                    x2, y2);
                                HandleClippedEdge(scanline, x, e, x2, y2,
                                    x3, y3);
                            }
                            else if (x3 < x1 && x0 > x2)
                            {
                                HandleClippedEdge(scanline, x, e, x0, y0,
                                    x2, y2);
                                HandleClippedEdge(scanline, x, e, x2, y2,
                                    x1, y1);
                                HandleClippedEdge(scanline, x, e, x1, y1,
                                    x3, y3);
                            }
                            else if (x0 < x1 && x3 > x1)
                            {
                                HandleClippedEdge(scanline, x, e, x0, y0,
                                    x1, y1);
                                HandleClippedEdge(scanline, x, e, x1, y1,
                                    x3, y3);
                            }
                            else if (x3 < x1 && x0 > x1)
                            {
                                HandleClippedEdge(scanline, x, e, x0, y0,
                                    x1, y1);
                                HandleClippedEdge(scanline, x, e, x1, y1,
                                    x3, y3);
                            }
                            else if (x0 < x2 && x3 > x2)
                            {
                                HandleClippedEdge(scanline, x, e, x0, y0,
                                    x2, y2);
                                HandleClippedEdge(scanline, x, e, x2, y2,
                                    x3, y3);
                            }
                            else if (x3 < x2 && x0 > x2)
                            {
                                HandleClippedEdge(scanline, x, e, x0, y0,
                                    x2, y2);
                                HandleClippedEdge(scanline, x, e, x2, y2,
                                    x3, y3);
                            }
                            else
                            {
                                HandleClippedEdge(scanline, x, e, x0, y0,
                                    x3, y3);
                            }
                        }
                    }
                }

                e = e.Next;
            }
        }

        private static void QuickSortEdges(Span<StbRendererGlyphEdge> p, int n)
        {
            while (n > 12)
            {
                StbRendererGlyphEdge t;
                int m = n >> 1;
                bool c01 = p[0].Y < p[m].Y;
                bool c12 = p[m].Y < p[n - 1].Y;
                if (c01 != c12)
                {
                    bool c = p[0].Y < p[n - 1].Y;
                    int z = c == c12 ? 0 : n - 1;
                    t = p[z];
                    p[z] = p[m];
                    p[m] = t;
                }

                t = p[0];
                p[0] = p[m];
                p[m] = t;
                var i = 1;
                int j = n - 1;
                while (true)
                {
                    while (true)
                    {
                        if (!(p[i].Y < p[0].Y))
                            break;

                        i++;
                    }

                    while (true)
                    {
                        if (!(p[0].Y < p[j].Y))
                            break;

                        j--;
                    }

                    if (i >= j)
                        break;
                    t = p[i];
                    p[i] = p[j];
                    p[j] = t;
                    i++;
                    j--;
                }

                if (j < n - i)
                {
                    QuickSortEdges(p, j);
                    p = p.Slice(i);
                    n -= i;
                }
                else
                {
                    QuickSortEdges(p.Slice(i), n - i);
                    n = j;
                }
            }
        }

        private static Vector2[]? FlattenCurves(GlyphDrawCommand[]? commands, float flatness, out int[]? contourLengthsOut)
        {
            contourLengthsOut = null;

            // No polygons found. This is actually a valid case. Fonts amirite?
            if (commands == null || commands.Length == 0) return null;

            float flatnessSquared = flatness * flatness;
            var points = new List<Vector2>();
            var contourLengths = new List<int>();

            var pointCount = 0;
            int contourStartIndex = -1;
            Vector2 prevPos = Vector2.Zero;
            for (var i = 0; i < commands.Length; i++)
            {
                GlyphDrawCommand command = commands[i];
                if (contourStartIndex == -1) contourStartIndex = i;

                switch (command.Type)
                {
                    case GlyphDrawCommandType.Move:
                        points.Add(command.P0);
                        pointCount++;
                        break;
                    case GlyphDrawCommandType.Line:
                        points.Add(command.P0);
                        pointCount++;
                        break;
                    case GlyphDrawCommandType.Curve:
                        TessellateCurve(points, ref pointCount, prevPos.X, prevPos.Y, command.P1.X, command.P1.Y, command.P0.X, command.P0.Y, flatnessSquared, 0);
                        break;
                    case GlyphDrawCommandType.Close:
                        command = commands[contourStartIndex];
                        points.Add(command.P0);
                        pointCount++;
                        contourLengths.Add(pointCount);

                        contourStartIndex = -1;
                        pointCount = 0;
                        break;
                }

                prevPos = command.P0;
            }

            contourLengthsOut = contourLengths.ToArray();
            return points.ToArray();
        }

        private static void TessellateCurve(List<Vector2> points, ref int pointIndex, float x0, float y0, float x1, float y1, float x2, float y2, float flatnessSquared, int n)
        {
            float mx = (x0 + 2 * x1 + x2) / 4;
            float my = (y0 + 2 * y1 + y2) / 4;
            float dx = (x0 + x2) / 2 - mx;
            float dy = (y0 + y2) / 2 - my;

            if (n > 16) return;

            if (dx * dx + dy * dy > flatnessSquared)
            {
                TessellateCurve(points, ref pointIndex, x0, y0, (x0 + x1) / 2.0f,
                    (y0 + y1) / 2.0f, mx, my, flatnessSquared,
                    n + 1);
                TessellateCurve(points, ref pointIndex, mx, my, (x1 + x2) / 2.0f,
                    (y1 + y2) / 2.0f, x2, y2, flatnessSquared,
                    n + 1);
            }
            else
            {
                points.Add(new Vector2(x2, y2));
                pointIndex++;
            }
        }

        private static void TessellateCurveCubic(List<Vector2> points, ref int pointIndex, float x0, float y0, float x1,
            float y1, float x2, float y2, float x3, float y3, float flatnessSquared, int n)
        {
            float dx0 = x1 - x0;
            float dy0 = y1 - y0;
            float dx1 = x2 - x1;
            float dy1 = y2 - y1;
            float dx2 = x3 - x2;
            float dy2 = y3 - y2;
            float dx = x3 - x0;
            float dy = y3 - y0;
            var longLength = (float) (Math.Sqrt(dx0 * dx0 + dy0 * dy0) +
                                      Math.Sqrt(dx1 * dx1 + dy1 * dy1) +
                                      Math.Sqrt(dx2 * dx2 + dy2 * dy2));
            var shortLength = (float) Math.Sqrt(dx * dx + dy * dy);
            float flatnessSquaredLocal = longLength * longLength - shortLength * shortLength;
            if (n > 16)
                return;
            if (flatnessSquaredLocal > flatnessSquared)
            {
                float x01 = (x0 + x1) / 2;
                float y01 = (y0 + y1) / 2;
                float x12 = (x1 + x2) / 2;
                float y12 = (y1 + y2) / 2;
                float x23 = (x2 + x3) / 2;
                float y23 = (y2 + y3) / 2;
                float xa = (x01 + x12) / 2;
                float ya = (y01 + y12) / 2;
                float xb = (x12 + x23) / 2;
                float yb = (y12 + y23) / 2;
                float mx = (xa + xb) / 2;
                float my = (ya + yb) / 2;
                TessellateCurveCubic(points, ref pointIndex, x0, y0, x01, y01,
                    xa, ya, mx, my, flatnessSquared,
                    n + 1);
                TessellateCurveCubic(points, ref pointIndex, mx, my, xb, yb,
                    x23, y23, x3, y3, flatnessSquared,
                    n + 1);
            }
            else
            {
                points.Add(new Vector2(x3, y3));
                pointIndex++;
            }
        }
    }
}