#region Using

using System;
using System.Numerics;
using Emotion.Game.Physics2D.Shape;
using Emotion.Game.QuadTree;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.Physics2D.Actors
{
    /// <summary>
    /// A link connected a body and its shapes.
    /// Handles collision logic for the attached shape. Such as what it collides with etc.
    /// </summary>
    public class PhysicsLink : IQuadTreeObject, IDisposable
    {
        /// <summary>
        /// The body being linked.
        /// </summary>
        public PhysicsBody Body { get; private set; }

        /// <summary>
        /// The shape linked to the body, used for collision detection.
        /// </summary>
        public ShapeBase Shape { get; private set; }

        /// <summary>
        /// Sliding resistance
        /// </summary>
        public float Friction { get; set; } = 0.2f;

        /// <summary>
        /// Bounciness [0-1]
        /// 0 means it wont bounce
        /// 1 means it will bounce back at full velocity
        /// </summary>
        public float Restitution { get; set; } = 0.0f;

        /// <summary>
        /// Minimum velocity to bounce
        /// </summary>
        public float RestitutionThreshold = 1.0f;

        public PhysicsLink(PhysicsBody body, ShapeBase shape)
        {
            Shape = shape;
            Body = body;
            UpdateBounds();
        }

        public void Dispose()
        {
            Shape = null;
            Body = null;
            _cachedBounds = Rectangle.Empty;
        }

        private Rectangle _cachedBounds = Rectangle.Empty;

        public void UpdateBounds()
        {
            PhysicsTransform transformSrc = Body.BodyTransform;
            PhysicsTransform transformDst;
            transformDst = Body.Flags.HasFlag(BodyFlags.Awake) ? Body.Sweep.GetTransform(0.0f) : Body.BodyTransform;

            const float boundsMargin = 0.1f;

            // Compute a bound that covers the swept Shape (may miss some rotation effect).
            Rectangle boundAtSrc = Shape.GetBounds(transformSrc);
            Rectangle boundAtDst = Shape.GetBounds(transformDst);

            Rectangle sweptBound = boundAtSrc.Union(boundAtDst);
            Vector2 displacement = boundAtDst.Center - boundAtSrc.Center;

            sweptBound.Inflate(boundsMargin, boundsMargin);

            // Predict movement
            Vector2 d = boundsMargin * displacement;
            if (d.X < 0.0f)
            {
                // Sign is swapped cuz negative.
                sweptBound.X += d.X;
                sweptBound.Width -= d.X;
            }
            else
            {
                sweptBound.Width += d.X;
            }

            if (d.Y < 0.0f)
            {
                sweptBound.Y += d.Y;
                sweptBound.Height -= d.Y;
            }
            else
            {
                sweptBound.Height += d.Y;
            }

            _cachedBounds = sweptBound;
            Moved();
        }

        #region QuadTreeObject

        public event EventHandler OnMove;
        public event EventHandler OnResize;

        public void Moved()
        {
            PhysicsWorld world = Body.World;
            if (world != null && world.MovingLinkHash.Add(this))
                world.MovingLinks.Add(this);
            OnMove?.Invoke(null, EventArgs.Empty);
        }

        public Rectangle GetBoundsForQuadTree()
        {
            return _cachedBounds;
        }

        #endregion
    }
}