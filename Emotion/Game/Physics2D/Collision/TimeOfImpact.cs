#region Using

using System;
using System.Diagnostics;
using System.Numerics;
using Emotion.Game.Physics2D.Actors;
using Emotion.Game.Physics2D.Distance;
using Emotion.Game.Physics2D.Shape;

#endregion

namespace Emotion.Game.Physics2D.Collision
{
    public enum TOIOutputState
    {
        Unknown,
        Failed,
        Overlapped,
        Touching,
        Seperated
    }

    public struct TOIOutput
    {
        public TOIOutputState State;
        public float T;
    }

    /// <summary>A distance proxy is used by the GJK algorithm. It encapsulates any shape.</summary>
    public struct DistanceProxy
    {
        internal readonly float _radius;
        internal readonly Vector2[] _vertices;

        public DistanceProxy(ShapeBase shape)
        {
            switch (shape)
            {
                case PolygonShape polyShape:
                {
                    _vertices = new Vector2[polyShape.Polygon.Vertices.Length];

                    for (int i = 0; i < polyShape.Polygon.Vertices.Length; i++)
                    {
                        _vertices[i] = polyShape.Polygon.Vertices[i];
                    }

                    _radius = polyShape.Radius;
                }
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        public DistanceProxy(Vector2[] vertices, float radius)
        {
            _vertices = vertices;
            _radius = radius;
        }

        /// <summary>Get the supporting vertex index in the given direction.</summary>
        /// <param name="direction">The direction.</param>
        public int GetSupport(Vector2 direction)
        {
            int bestIndex = 0;
            float bestValue = Vector2.Dot(_vertices[0], direction);
            for (int i = 1; i < _vertices.Length; ++i)
            {
                float value = Vector2.Dot(_vertices[i], direction);
                if (value > bestValue)
                {
                    bestIndex = i;
                    bestValue = value;
                }
            }

            return bestIndex;
        }

        public Vector2 GetVertex(int index)
        {
            Debug.Assert(0 <= index && index < _vertices.Length);
            return _vertices[index];
        }
    }

    /// <summary>Input parameters for CalculateTimeOfImpact</summary>
    public struct TOIInput
    {
        public DistanceProxy ProxyA;
        public DistanceProxy ProxyB;
        public Sweep SweepA;
        public Sweep SweepB;
        public float TMax; // defines sweep interval [0, tMax]
    }

    public static class TimeOfImpact
    {
        // CCD via the local separating axis method. This seeks progression
        // by computing the largest time at which separation is maintained.
        /// <summary>
        /// Compute the upper bound on time before two shapes penetrate. Time is represented as a fraction between
        /// [0,tMax]. This uses a swept separating axis and may miss some intermediate, non-tunneling collision. If you change the
        /// time interval, you should call this function again. Note: use Distance() to compute the contact point and normal at the
        /// time of impact.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        public static void CalculateTimeOfImpact(ref TOIInput input, out TOIOutput output)
        {
            output = new TOIOutput();
            output.State = TOIOutputState.Unknown;
            output.T = input.TMax;

            Sweep sweepA = input.SweepA;
            Sweep sweepB = input.SweepB;

            // Large rotations can make the root finder fail, so we normalize the
            // sweep angles.
            sweepA.NormalizeAngles();
            sweepB.NormalizeAngles();

            float tMax = input.TMax;

            float totalRadius = input.ProxyA._radius + input.ProxyB._radius;
            float target = Math.Max(PhysicsConfig.LinearSlop, totalRadius - 3.0f * PhysicsConfig.LinearSlop);
            float tolerance = 0.25f * PhysicsConfig.LinearSlop;
            Debug.Assert(target > tolerance);

            float t1 = 0.0f;
            const int k_maxIterations = 20;
            int iter = 0;

            // Prepare input for distance query.
            DistanceInput distanceInput = new DistanceInput();
            distanceInput.ProxyA = input.ProxyA;
            distanceInput.ProxyB = input.ProxyB;
            distanceInput.UseRadii = false;

            // The outer loop progressively attempts to compute new separating axes.
            // This loop terminates when an axis is repeated (no progress is made).
            while(true)
            {
                PhysicsTransform xfA = sweepA.GetTransform(t1);
                PhysicsTransform xfB = sweepB.GetTransform(t1);

                // Get the distance between shapes. We can also use the results
                // to get a separating axis.
                distanceInput.TransformA = xfA;
                distanceInput.TransformB = xfB;
                DistanceGJK.ComputeDistance(ref distanceInput, out DistanceOutput distanceOutput, out SimplexCache cache);

                // If the shapes are overlapped, we give up on continuous collision.
                if (distanceOutput.Distance <= 0.0f)
                {
                    // Failure!
                    output.State = TOIOutputState.Overlapped;
                    output.T = 0.0f;
                    break;
                }

                if (distanceOutput.Distance < target + tolerance)
                {
                    // Victory!
                    output.State = TOIOutputState.Touching;
                    output.T = t1;
                    break;
                }

                Initialize(ref cache, input.ProxyA, ref sweepA, input.ProxyB, ref sweepB, t1, out Vector2 axis, out Vector2 localPoint, out SeparationFunctionType type);

                // Compute the TOI on the separating axis. We do this by successively
                // resolving the deepest point. This loop is bounded by the number of vertices.
                var done = false;
                float t2 = tMax;
                var pushBackIter = 0;
                while(true)
                {
                    // Find the deepest point at t2. Store the witness point indices.
                    float s2 = FindMinSeparation(out int indexA, out int indexB, t2, input.ProxyA, ref sweepA, input.ProxyB, ref sweepB, ref axis, ref localPoint, type);

                    // Is the final configuration separated?
                    if (s2 > target + tolerance)
                    {
                        // Victory!
                        output.State = TOIOutputState.Seperated;
                        output.T = tMax;
                        done = true;
                        break;
                    }

                    // Has the separation reached tolerance?
                    if (s2 > target - tolerance)
                    {
                        // Advance the sweeps
                        t1 = t2;
                        break;
                    }

                    // Compute the initial separation of the witness points.
                    float s1 = Evaluate(indexA, indexB, t1, input.ProxyA, ref sweepA, input.ProxyB, ref sweepB, ref axis, ref localPoint, type);

                    // Check for initial overlap. This might happen if the root finder
                    // runs out of iterations.
                    if (s1 < target - tolerance)
                    {
                        output.State = TOIOutputState.Failed;
                        output.T = t1;
                        done = true;
                        break;
                    }

                    // Check for touching
                    if (s1 <= target + tolerance)
                    {
                        // Victory! t1 should hold the TOI (could be 0.0).
                        output.State = TOIOutputState.Touching;
                        output.T = t1;
                        done = true;
                        break;
                    }

                    // Compute 1D root of: f(x) - target = 0
                    var rootIterCount = 0;
                    float a1 = t1, a2 = t2;
                    while (true)
                    {
                        // Use a mix of the secant rule and bisection.
                        float t;
                        if ((rootIterCount & 1) != 0)
                            // Secant rule to improve convergence.
                            t = a1 + (target - s1) * (a2 - a1) / (s2 - s1);
                        else
                            // Bisection to guarantee progress.
                            t = 0.5f * (a1 + a2);

                        ++rootIterCount;

                        float s = Evaluate(indexA, indexB, t, input.ProxyA, ref sweepA, input.ProxyB, ref sweepB, ref axis, ref localPoint, type);

                        if (Math.Abs(s - target) < tolerance)
                        {
                            // t2 holds a tentative value for t1
                            t2 = t;
                            break;
                        }

                        // Ensure we continue to bracket the root.
                        if (s > target)
                        {
                            a1 = t;
                            s1 = s;
                        }
                        else
                        {
                            a2 = t;
                            s2 = s;
                        }

                        if (rootIterCount == 50)
                            break;
                    }

                    ++pushBackIter;

                    if (pushBackIter == PhysicsConfig.MaxPolygonVertices)
                        break;
                }

                ++iter;

                if (done)
                    break;

                if (iter == k_maxIterations)
                {
                    // Root finder got stuck. Semi-victory.
                    output.State = TOIOutputState.Failed;
                    output.T = t1;
                    break;
                }
            }
        }

        public enum SeparationFunctionType
        {
            Points,
            FaceA,
            FaceB
        }

        public static void Initialize(ref SimplexCache cache, DistanceProxy proxyA, ref Sweep sweepA, DistanceProxy proxyB, ref Sweep sweepB, float t1, out Vector2 axis, out Vector2 localPoint,
            out SeparationFunctionType type)
        {
            int count = cache.Count;
            Debug.Assert(0 < count && count < 3);

            PhysicsTransform xfA = sweepA.GetTransform(t1);
            PhysicsTransform xfB = sweepB.GetTransform(t1);

            if (count == 1)
            {
                localPoint = Vector2.Zero;
                type = SeparationFunctionType.Points;
                Vector2 localPointA = proxyA._vertices[cache.IndexA[0]];
                Vector2 localPointB = proxyB._vertices[cache.IndexB[0]];
                Vector2 pointA = xfA.TransformVector(localPointA);
                Vector2 pointB = xfB.TransformVector(localPointB);
                axis = pointB - pointA;
                axis.Normalize();
            }
            else if (cache.IndexA[0] == cache.IndexA[1])
            {
                // Two points on B and one on A.
                type = SeparationFunctionType.FaceB;
                Vector2 localPointB1 = proxyB._vertices[cache.IndexB[0]];
                Vector2 localPointB2 = proxyB._vertices[cache.IndexB[1]];

                Vector2 a = localPointB2 - localPointB1;
                axis = new Vector2(a.Y, -a.X);
                axis.Normalize();
                Vector2 normal = xfB.RotateVector(axis);

                localPoint = 0.5f * (localPointB1 + localPointB2);
                Vector2 pointB = xfB.TransformVector(localPoint);

                Vector2 localPointA = proxyA._vertices[cache.IndexA[0]];
                Vector2 pointA = xfA.TransformVector(localPointA);

                float s = Vector2.Dot(pointA - pointB, normal);
                if (s < 0.0f)
                    axis = -axis;
            }
            else
            {
                // Two points on A and one or two points on B.
                type = SeparationFunctionType.FaceA;
                Vector2 localPointA1 = proxyA._vertices[cache.IndexA[0]];
                Vector2 localPointA2 = proxyA._vertices[cache.IndexA[1]];

                Vector2 a = localPointA2 - localPointA1;
                axis = new Vector2(a.Y, -a.X);
                axis.Normalize();
                Vector2 normal = xfA.RotateVector(axis);

                localPoint = 0.5f * (localPointA1 + localPointA2);
                Vector2 pointA = xfA.TransformVector(localPoint);

                Vector2 localPointB = proxyB._vertices[cache.IndexB[0]];
                Vector2 pointB = xfB.TransformVector(localPointB);

                float s = Vector2.Dot(pointB - pointA, normal);
                if (s < 0.0f)
                    axis = -axis;
            }

            //Velcro note: the returned value that used to be here has been removed, as it was not used.
        }

        public static float FindMinSeparation(out int indexA, out int indexB, float t, DistanceProxy proxyA, ref Sweep sweepA, DistanceProxy proxyB, ref Sweep sweepB, ref Vector2 axis,
            ref Vector2 localPoint, SeparationFunctionType type)
        {
            PhysicsTransform xfA = sweepA.GetTransform(t);
            PhysicsTransform xfB = sweepB.GetTransform(t);

            switch (type)
            {
                case SeparationFunctionType.Points:
                {
                    Vector2 axisA = xfA.RotateTransposeVector(axis);
                    Vector2 axisB = xfB.RotateTransposeVector(-axis);

                    indexA = proxyA.GetSupport(axisA);
                    indexB = proxyB.GetSupport(axisB);

                    Vector2 localPointA = proxyA._vertices[indexA];
                    Vector2 localPointB = proxyB._vertices[indexB];

                    Vector2 pointA = xfA.TransformVector(localPointA);
                    Vector2 pointB = xfB.TransformVector(localPointB);

                    float separation = Vector2.Dot(pointB - pointA, axis);
                    return separation;
                }

                case SeparationFunctionType.FaceA:
                {
                    Vector2 normal = xfA.RotateVector(axis);
                    Vector2 pointA = xfA.TransformVector(localPoint);

                    Vector2 axisB = xfB.RotateTransposeVector(-normal);

                    indexA = -1;
                    indexB = proxyB.GetSupport(axisB);

                    Vector2 localPointB = proxyB._vertices[indexB];
                    Vector2 pointB = xfB.TransformVector(localPointB);

                    float separation = Vector2.Dot(pointB - pointA, normal);
                    return separation;
                }

                case SeparationFunctionType.FaceB:
                {
                    Vector2 normal = xfB.RotateVector(axis);
                    Vector2 pointB = xfB.TransformVector(localPoint);

                    Vector2 axisA = xfA.RotateTransposeVector(-normal);

                    indexB = -1;
                    indexA = proxyA.GetSupport(axisA);

                    Vector2 localPointA = proxyA._vertices[indexA];
                    Vector2 pointA = xfA.TransformVector(localPointA);

                    float separation = Vector2.Dot(pointA - pointB, normal);
                    return separation;
                }

                default:
                    Debug.Assert(false);
                    indexA = -1;
                    indexB = -1;
                    return 0.0f;
            }
        }

        public static float Evaluate(int indexA, int indexB, float t, DistanceProxy proxyA, ref Sweep sweepA, DistanceProxy proxyB, ref Sweep sweepB, ref Vector2 axis, ref Vector2 localPoint,
            SeparationFunctionType type)
        {
            PhysicsTransform xfA = sweepA.GetTransform(t);
            PhysicsTransform xfB = sweepB.GetTransform(t);

            switch (type)
            {
                case SeparationFunctionType.Points:
                {
                    Vector2 localPointA = proxyA._vertices[indexA];
                    Vector2 localPointB = proxyB._vertices[indexB];

                    Vector2 pointA = xfA.TransformVector(localPointA);
                    Vector2 pointB = xfB.TransformVector(localPointB);
                    float separation = Vector2.Dot(pointB - pointA, axis);

                    return separation;
                }
                case SeparationFunctionType.FaceA:
                {
                    Vector2 normal = xfA.RotateVector(axis);
                    Vector2 pointA = xfA.TransformVector(localPoint);

                    Vector2 localPointB = proxyB._vertices[indexB];
                    Vector2 pointB = xfB.TransformVector(localPointB);

                    float separation = Vector2.Dot(pointB - pointA, normal);
                    return separation;
                }
                case SeparationFunctionType.FaceB:
                {
                    Vector2 normal = xfB.RotateVector(axis);
                    Vector2 pointB = xfB.TransformVector(localPoint);

                    Vector2 localPointA = proxyA._vertices[indexA];
                    Vector2 pointA = xfA.TransformVector(localPointA);

                    float separation = Vector2.Dot(pointA - pointB, normal);
                    return separation;
                }
                default:
                    Debug.Assert(false);
                    return 0.0f;
            }
        }
    }
}