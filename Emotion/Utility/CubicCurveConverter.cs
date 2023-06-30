#region Using

using System;
using System.Collections.Generic;
using System.Numerics;

#endregion

namespace Emotion.Utility
{
    /// <summary>
    /// Converts cubic curved to quadratic curves.
    /// Used by the font parser.
    /// From https://github.com/fontello/cubic2quad
    /// </summary>
    public static class CubicCurveConverter
    {
        public static float Precision = 0.1f;

        public static List<Vector2> CubicToQuad(Vector2 p1, Vector2 cp1, Vector2 cp2, Vector2 p2)
        {
            float[] inflections = SolveInflections(p1, cp1, cp2, p2);
            if (inflections.Length == 0) return CubicToQuadInternal(p1, cp1, cp2, p2);

            Vector2[] curve = { p1, cp1, cp2, p2 };
            var result = new List<Vector2>();
            float prevPoint = 0;
            for (var inflectionIdx = 0; inflectionIdx < inflections.Length; inflectionIdx++)
            {
                // we make a new curve, so adjust inflection point accordingly
                float t = 1 - (1 - inflections[inflectionIdx]) / (1f - prevPoint);
                Vector2[][] split = SubdivideCubic(curve[0], curve[1], curve[2], curve[3], t);

                Vector2[] firstCurve = split[0];
                List<Vector2> quadCurve = CubicToQuadInternal(firstCurve[0], firstCurve[1], firstCurve[2], firstCurve[3]);

                for (var i = 0; i < quadCurve.Count - 1; i++)
                {
                    result.Add(quadCurve[i]);
                }

                curve = split[1];
                prevPoint = inflections[inflectionIdx];
            }

            List<Vector2> quad = CubicToQuadInternal(curve[0], curve[1], curve[2], curve[3]);
            result.AddRange(quad);

            return result;
        }

        /// <summary>
        /// Find inflection points on a cubic curve, algorithm is similar to this one:
        /// http://www.caffeineowl.com/graphics/2d/vectorial/cubic-inflexion.html
        /// </summary>
        private static float[] SolveInflections(Vector2 p1, Vector2 cp1, Vector2 cp2, Vector2 p2)
        {
            float p = -(p2.X * (p1.Y - 2f * cp1.Y + cp2.Y)) +
                      cp2.X * (2f * p1.Y - 3f * cp1.Y + p2.Y) +
                      p1.X * (cp1.Y - 2f * cp2.Y + p2.Y) -
                      cp1.X * (p1.Y - 3f * cp2.Y + 2f * p2.Y);

            float q = p2.X * (p1.Y - cp1.Y) +
                3f * cp2.X * (-p1.Y + cp1.Y) + cp1.X *
                (2f * p1.Y - 3f * cp2.Y + p2.Y) - p1.X *
                (2f * cp1.Y - 3f * cp2.Y + p2.Y);

            float r = cp2.X * (p1.Y - cp1.Y) + p1.X * (cp1.Y - cp2.Y) + cp1.X * (-p1.Y + cp2.Y);

            float[] result = QuadSolve(p, q, r);
            float[] filteredResult = Array.Empty<float>();
            for (var i = 0; i < result.Length; i++)
            {
                float t = result[i];
                if (t > Maths.EPSILON && t < 1f - Maths.EPSILON) filteredResult = filteredResult.AddToArray(t);
            }

            Array.Sort(filteredResult);
            return filteredResult;
        }

        // a*x^2 + b*x + c = 0
        private static float[] QuadSolve(float a, float b, float c)
        {
            if (a == 0) return b == 0 ? Array.Empty<float>() : new[] { -c / b };

            float d = b * b - 4 * a * c;
            if (MathF.Abs(d) < Maths.EPSILON) return new[] { -b / (2 * a) };

            if (d < 0) return Array.Empty<float>();

            float dSqrt = MathF.Sqrt(d);
            float first = (-b - dSqrt) / (2 * a);
            float second = (-b + dSqrt) / (2 * a);
            return new[] { first, second };
        }

        /// <summary>
        /// Approximate cubic Bezier curve defined with base points p1, p2 and control points c1, c2 with
        /// with a few quadratic Bezier curves.
        /// The function uses tangent method to find quadratic approximation of cubic curve segment and
        /// simplified Hausdorff distance to determine number of segments that is enough to make error small.
        /// In general the method is the same as described here: https://fontforge.github.io/bezier.html.
        /// </summary>
        private static List<Vector2> CubicToQuadInternal(Vector2 p1, Vector2 c1, Vector2 c2, Vector2 p2)
        {
            (Vector2, Vector2, Vector2, Vector2) pc = CalcPowerCoefficients(p1, c1, c2, p2);
            Vector2 a = pc.Item1;
            Vector2 b = pc.Item2;
            Vector2 c = pc.Item3;
            Vector2 d = pc.Item4;

            var approximation = new List<Vector2>();
            for (var segmentsCount = 1; segmentsCount <= 8; segmentsCount++)
            {
                approximation.Clear();
                for (float t = 0; t < 1; t += 1f / segmentsCount)
                {
                    approximation.AddRange(ProcessSegment(a, b, c, d, t, t + 1f / segmentsCount));
                }

                // approximation concave, while the curve is convex (or vice versa)
                if (segmentsCount == 1)
                {
                    float approx1 = Vector2.Dot(approximation[1] - p1, c1 - p1);
                    float approx2 = Vector2.Dot(approximation[1] - p2, c2 - p2);
                    if (approx1 < 0 || approx2 < 0) continue;
                }

                if (IsApproximationClose(a, b, c, d, approximation)) break;
            }

            var result = new List<Vector2>
            {
                approximation[0]
            };
            for (var i = 0; i < approximation.Count; i += 3)
            {
                result.Add(approximation[i + 1]);
                result.Add(approximation[i + 2]);
            }

            return result;
        }

        // point(t) = p1*(1-t)^3 + c1*t*(1-t)^2 + c2*t^2*(1-t) + p2*t^3 = a*t^3 + b*t^2 + c*t + d
        // for each t value, so
        // a = (p2 - p1) + 3 * (c1 - c2)
        // b = 3 * (p1 + c2) - 6 * c1
        // c = 3 * (c1 - p1)
        // d = p1
        private static (Vector2, Vector2, Vector2, Vector2) CalcPowerCoefficients(Vector2 p1, Vector2 c1, Vector2 c2, Vector2 p2)
        {
            Vector2 a = p2 - p1 + (c1 - c2) * 3f;
            Vector2 b = (p1 + c2) * 3 - c1 * 6f;
            Vector2 c = (c1 - p1) * 3;
            Vector2 d = p1;
            return (a, b, c, d);
        }

        // point(t) = p1*(1-t)^2 + c1*t*(1-t) + p2*t^2 = a*t^2 + b*t + c
        // for each t value, so
        // a = p1 + p2 - 2 * c1
        // b = 2 * (c1 - p1)
        // c = p1
        private static (Vector2, Vector2, Vector2) CalcPowerCoefficientsQuad(Vector2 p1, Vector2 c1, Vector2 p2)
        {
            Vector2 a = c1 * -2 + p1 + p2;
            Vector2 b = (c1 - p1) * 2;
            Vector2 c = p1;
            return (a, b, c);
        }

        /// <summary>
        /// Find a single control point for given segment of cubic Bezier curve
        /// These control point is an interception of tangent lines to the boundary points
        /// Let's denote that f(t) is a vector function of parameter t that defines the cubic Bezier curve,
        /// f(t1) + f'(t1)*z1 is a parametric equation of tangent line to f(t1) with parameter z1
        /// f(t2) + f'(t2)*z2 is the same for point f(t2) and the vector equation
        /// f(t1) + f'(t1)*z1 = f(t2) + f'(t2)*z2 defines the values of parameters z1 and z2.
        /// Defining fx(t) and fy(t) as the x and y components of vector function f(t) respectively
        /// and solving the given system for z1 one could obtain that
        /// -(fx(t2) - fx(t1))*fy'(t2) + (fy(t2) - fy(t1))*fx'(t2)
        /// z1 = ------------------------------------------------------.
        /// -fx'(t1)*fy'(t2) + fx'(t2)*fy'(t1)
        /// Let's assign letter D to the denominator and note that if D = 0 it means that the curve actually
        /// is a line. Substituting z1 to the equation of tangent line to the point f(t1), one could obtain that
        /// cx = [fx'(t1)*(fy(t2)*fx'(t2) - fx(t2)*fy'(t2)) + fx'(t2)*(fx(t1)*fy'(t1) - fy(t1)*fx'(t1))]/D
        /// cy = [fy'(t1)*(fy(t2)*fx'(t2) - fx(t2)*fy'(t2)) + fy'(t2)*(fx(t1)*fy'(t1) - fy(t1)*fx'(t1))]/D
        /// where c = (cx, cy) is the control point of quadratic Bezier curve.
        /// </summary>
        private static Vector2[] ProcessSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t1, float t2)
        {
            Vector2 f1 = CalcPoint(a, b, c, d, t1);
            Vector2 f2 = CalcPoint(a, b, c, d, t2);
            Vector2 f1D = CalcPointDerivative(a, b, c, t1);
            Vector2 f2D = CalcPointDerivative(a, b, c, t2);

            float der = -f1D.X * f2D.Y + f2D.X * f1D.Y;

            // straight line segment
            if (MathF.Abs(der) < Maths.EPSILON)
                return new[] { f1, (f1 + f2) / 2f, f2 };

            float cx = (f1D.X * (f2.Y * f2D.X - f2.X * f2D.Y) + f2D.X * (f1.X * f1D.Y - f1.Y * f1D.X)) / der;
            float cy = (f1D.Y * (f2.Y * f2D.X - f2.X * f2D.Y) + f2D.Y * (f1.X * f1D.Y - f1.Y * f1D.X)) / der;
            return new[] { f1, new Vector2(cx, cy), f2 };
        }

        // a*t^3 + b*t^2 + c*t + d = ((a*t + b)*t + c)*t + d
        private static Vector2 CalcPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
        {
            Vector2 result = a * t;
            result += b;
            result *= t;
            result += c;
            result *= t;
            result += d;
            return result;
        }

        // a*t^2 + b*t + c = (a*t + b)*t + c
        private static Vector2 CalcPointQuad(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            return (a * t + b) * t + c;
        }

        // d/dt[a*t^3 + b*t^2 + c*t + d] = 3*a*t^2 + 2*b*t + c = (3*a*t + 2*b)*t + c
        private static Vector2 CalcPointDerivative(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            Vector2 result = a * (3 * t);
            result += b * 2;
            result *= t;
            result += c;
            return result;
        }

        private static bool IsApproximationClose(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<Vector2> quadCurves)
        {
            float dt = 1f / (quadCurves.Count / 3f);
            for (var i = 0; i < quadCurves.Count; i += 3)
            {
                Vector2 p1 = quadCurves[i];
                Vector2 c1 = quadCurves[i + 1];
                Vector2 p2 = quadCurves[i + 2];

                int curveIndex = i / 3;
                if (!IsSegmentApproximationClose(a, b, c, d, curveIndex * dt, (curveIndex + 1) * dt, p1, c1, p2)) return false;
            }

            return true;
        }

        /// <summary>
        /// Divide cubic and quadratic curves into 10 points and 9 line segments.
        /// Calculate distances between each point on cubic and nearest line segment
        /// on quadratic (and vice versa), and make sure all distances are less
        /// than `errorBound`.
        /// We need to calculate BOTH distance from all points on quadratic to any cubic,
        /// and all points on cubic to any quadratic.
        /// If we do it only one way, it may lead to an error if the entire original curve
        /// falls within errorBound (then **any** quad will erroneously treated as good):
        /// https://github.com/fontello/svg2ttf/issues/105#issuecomment-842558027
        /// - a,b,c,d define cubic curve (power coefficients)
        /// - tmin, tmax are boundary points on cubic curve (in 0-1 range)
        /// - p1, c1, p2 define quadratic curve (control points)
        /// - errorBound is maximum allowed distance
        /// </summary>
        private static bool IsSegmentApproximationClose(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float tmin, float tmax, Vector2 p1, Vector2 c1, Vector2 p2)
        {
            const float n = 10f; // number of points

            (Vector2, Vector2, Vector2) p = CalcPowerCoefficientsQuad(p1, c1, p2);
            Vector2 qa = p.Item1;
            Vector2 qb = p.Item2;
            Vector2 qc = p.Item3;

            float errorBoundSq = Precision * Precision;
            var cubicPoints = new List<Vector2>();
            var quadPoints = new List<Vector2>();

            float t, dt;
            int i, j;
            float distSq;
            float minDistSq;

            dt = (tmax - tmin) / n;
            for (i = 0, t = tmin; i <= n; i++, t += dt)
            {
                cubicPoints.Add(CalcPoint(a, b, c, d, t));
            }

            dt = 1f / n;
            for (i = 0, t = 0; i <= n; i++, t += dt)
            {
                quadPoints.Add(CalcPointQuad(qa, qb, qc, t));
            }

            for (i = 1; i < cubicPoints.Count - 1; i++)
            {
                minDistSq = float.PositiveInfinity;
                for (j = 0; j < quadPoints.Count - 1; j++)
                {
                    distSq = MinDistanceToLineSq(cubicPoints[i], quadPoints[j], quadPoints[j + 1]);
                    minDistSq = MathF.Min(minDistSq, distSq);
                }

                if (minDistSq > errorBoundSq) return false;
            }

            for (i = 1; i < quadPoints.Count - 1; i++)
            {
                minDistSq = float.PositiveInfinity;
                for (j = 0; j < cubicPoints.Count - 1; j++)
                {
                    distSq = MinDistanceToLineSq(quadPoints[i], cubicPoints[j], cubicPoints[j + 1]);
                    minDistSq = MathF.Min(minDistSq, distSq);
                }

                if (minDistSq > errorBoundSq) return false;
            }

            return true;
        }

        /// <summary>
        /// Calculate a distance between a `point` and a line segment `p1, p2`
        /// (result is squared for performance reasons), see details here:
        /// https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
        /// </summary>
        private static float MinDistanceToLineSq(Vector2 point, Vector2 p1, Vector2 p2)
        {
            Vector2 lineSize = p2 - p1;
            float dot = Vector2.Dot(point - p1, lineSize);
            float lenSq = lineSize.LengthSquared();
            float param = 0;
            if (lenSq != 0) param = dot / lenSq;

            Vector2 diff = param switch
            {
                <= 0 => point - p1,
                >= 1 => point - p2,
                _ => point - (p1 + lineSize * param)
            };
            return diff.LengthSquared();
        }

        /// <summary>
        /// Split cubic bézier curve into two cubic curves, see details here:
        /// https://math.stackexchange.com/questions/877725
        /// </summary>
        private static Vector2[][] SubdivideCubic(Vector2 p1, Vector2 cp1, Vector2 cp2, Vector2 p2, float t)
        {
            float u = 1 - t;

            Vector2 b = p1 * u + cp1 * t;
            Vector2 s = cp1 * u + cp2 * t;
            Vector2 f = cp2 * u + p2 * t;

            Vector2 c = b * u + s * t;
            Vector2 e = s * u + f * t;
            Vector2 d = c * u + e * t;

            return new[]
            {
                new[]
                {
                    p1,
                    b,
                    c,
                    d
                },
                new[]
                {
                    d,
                    e,
                    f,
                    p2
                },
            };
        }

        //public class ConverterQuadCurve
        //{
        //    public Vector2 P1;
        //    public Vector2 P2;
        //    public Vector2 CP;
        //}

        //public class ConverterCubicCurve
        //{
        //    public Vector2 P1;
        //    public Vector2 P2;
        //    public Vector2 CP;
        //    public Vector2 CP2;
        //}

        public static void Tests()
        {
            // cubic curve have to be converted to two or more quads for large precision
            Precision = 100f;
            List<Vector2> converted = CubicToQuad(new Vector2(0, 0), new Vector2(-5, 10), new Vector2(35, 10), new Vector2(30, 0));
            Debug.Assert(converted.Count > 3);

            // should split curve at inflection point
            Precision = 1000f;
            converted = CubicToQuad(new Vector2(0, 100), new Vector2(70, 0), new Vector2(30, 0), new Vector2(100, 100));

            // 1st inflection point
            Debug.Assert(MathF.Round(converted[2].X, 2) == 34.33f);
            Debug.Assert(MathF.Round(converted[2].Y, 2) == 45.45f);
            // 2nd inflection point
            Debug.Assert(MathF.Round(converted[4].X, 2) == 65.67f);
            Debug.Assert(MathF.Round(converted[4].Y, 2) == 45.45f);

            Precision = 0.5f;
            converted = CubicToQuad(new Vector2(858, -113), new Vector2(739, -68), new Vector2(624, -31), new Vector2(533, 0));
            // All points should be in the bounding box of the source curve
            for (var j = 0; j < converted.Count; j++)
            {
                Debug.Assert(converted[j].X <= 858);
                Debug.Assert(converted[j].X >= 533);
                Debug.Assert(converted[j].Y >= -113);
                Debug.Assert(converted[j].Y <= 0);
            }

            // cubic curve should be split due to small error margin
            Precision = 0.01f;
            converted = CubicToQuad(new Vector2(0, 0), new Vector2(10, 9), new Vector2(20, 11), new Vector2(30, 0));
            Debug.Assert(converted.Count == 7);

            // real error is 0.1, but our approximation increases error to 0.15
            Precision = 0.15f;
            converted = CubicToQuad(new Vector2(0, 0), new Vector2(10, 9), new Vector2(20, 11), new Vector2(30, 0));
            Debug.Assert(converted.Count == 3);

            // quadratic curve to the same quadratic curve (error ~ 1e-8)
            Precision = 1e-8f;
            converted = CubicToQuad(new Vector2(0, 0), new Vector2(10, 10), new Vector2(20, 10), new Vector2(30, 0));
            Debug.Assert(converted.Count == 3);
            Debug.Assert(converted[0].X == 0 && converted[0].Y == 0 && converted[1].X == 15 && converted[1].Y == 15 && converted[2].X == 30 && converted[2].Y == 0);

            // convert cubic Bezier curve to a number of quadratic ones
            Precision = 1e-8f;
            converted = CubicToQuad(new Vector2(0, 0), new Vector2(10, 10), new Vector2(20, 20), new Vector2(30, 30));
            Debug.Assert(converted.Count == 6 / 2);

            Debug.Assert(converted[0].X == 0);
            Debug.Assert(converted[0].Y == 0);
            Debug.Assert(converted[1].X == 15);
            Debug.Assert(converted[1].Y == 15);
            Debug.Assert(converted[2].Y == 30);
            Debug.Assert(converted[2].Y == 30);

            Precision = 0.1f;
        }
    }
}