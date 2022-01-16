#region Using

using System;
using System.Diagnostics;
using System.Numerics;
using Emotion.Game.Physics2D.Actors;
using Emotion.Utility;

#endregion

namespace Emotion.Game.Physics2D.Collision
{
    /// <summary>
    /// This describes the motion of a body/shape for TOI computation. Shapes are defined with respect to the body
    /// origin, which may no coincide with the center of mass. However, to support dynamics we must interpolate the center of
    /// mass position.
    /// </summary>
    public struct Sweep
    {
        /// <summary>
        /// Angle current
        /// </summary>
        public float Angle;

        /// <summary>
        /// Angle at 0
        /// </summary>
        public float Angle0;

        /// <summary>
        /// Fraction of the current time step in the range [0,1]
        /// </summary>
        public float Alpha0;

        /// <summary>
        /// Center world position
        /// </summary>
        public Vector2 CenterWorld;

        /// <summary>
        /// Center world position at 0
        /// </summary>
        public Vector2 CenterWorld0;

        /// <summary>
        /// Local center of mass position
        /// </summary>
        public Vector2 LocalCenter;

        /// <summary>
        /// Get the interpolated transform at a specific time.
        /// </summary>
        /// <param name="beta">Beta is a factor in [0,1], where 0 indicates alpha0.</param>
        public PhysicsTransform GetTransform(float beta)
        {
            var transform = new PhysicsTransform();
            SetTransform(transform, beta);
            return transform;
        }

        /// <summary>
        /// Sets the provided transform to the interpolated transform at the specified time.
        /// </summary>
        /// <param name="transform">The transform to set.</param>
        /// <param name="beta">Beta is a factor in [0,1], where 0 indicates alpha0.</param>
        public void SetTransform(PhysicsTransform transform, float beta)
        {
            transform.Position.X = (1.0f - beta) * CenterWorld0.X + beta * CenterWorld.X;
            transform.Position.Y = (1.0f - beta) * CenterWorld0.Y + beta * CenterWorld.Y;
            float angle = (1.0f - beta) * Angle0 + beta * Angle;
            transform.Rotation.SetAngle(angle);

            // Shift to origin
            transform.Position -= transform.RotateVector(LocalCenter);
        }

        /// <summary>Advance the sweep forward, yielding a new initial state.</summary>
        /// <param name="alpha">new initial time</param>
        public void Advance(float alpha)
        {
            Debug.Assert(Alpha0 < 1.0f);
            float beta = (alpha - Alpha0) / (1.0f - Alpha0);
            CenterWorld0 += beta * (CenterWorld - CenterWorld0);
            Angle0 += beta * (Angle - Angle0);
            Alpha0 = alpha;
        }

        /// <summary>
        /// Normalize the angles.
        /// </summary>
        public void NormalizeAngles()
        {
            float d = Maths.TWO_PI * (float) Math.Floor(Angle0 / Maths.TWO_PI);
            Angle0 -= d;
            Angle -= d;
        }
    }
}