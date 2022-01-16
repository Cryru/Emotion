#region Using

using System.Numerics;
using Emotion.Game.Physics2D.Actors;
using Emotion.Game.Physics2D.Collision;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.Physics2D.Shape
{
    /// <summary>
    /// Precomputed information about a shape.
    /// </summary>
    public class MassData
    {
        public float Area;
        public Vector2 Centroid;
        public float Inertia;
        public float Mass;
    }

    /// <summary>
    /// A shape for collision detection.
    /// </summary>
    public abstract class ShapeBase
    {
        public float Density { get; protected set; }
        public float Radius { get; protected set; }
        public MassData MassData { get; protected set; } = new MassData();

        protected ShapeBase(float density)
        {
            Density = density;
        }

        /// <summary>
        /// Given a transform, compute the associated axis aligned bounding box for a child shape.
        /// </summary>
        /// <param name="transform">The world transform of the shape.</param>
        public abstract Rectangle GetBounds(PhysicsTransform transform);

        /// <summary>
        /// Returns the type of the contact that could occur between these two shapes.
        /// </summary>
        public abstract ContactType GetContactType(ShapeBase otherShape);

        /// <summary>
        /// Compute the mass properties of this shape using its dimensions and density.
        /// The inertia tensor is computed about the local origin, not the centroid.
        /// </summary>
        protected abstract void ComputeMassData();
    }
}