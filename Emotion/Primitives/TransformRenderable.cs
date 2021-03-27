#region Using

using System.Numerics;
using Emotion.Graphics;

#endregion

namespace Emotion.Primitives
{
    /// <summary>
    /// A transform which inherits IRenderable
    /// </summary>
    public abstract class TransformRenderable : Transform, IRenderable
    {
        #region Constructors

        /// <summary>
        /// Create a new renderable transform from a rectangle.
        /// </summary>
        /// <param name="bounds">The rectangle to create from.</param>
        protected TransformRenderable(Rectangle bounds) : this(bounds.Location, bounds.Size)
        {
        }

        /// <summary>
        /// Create a new renderable transform from two vec2s.
        /// </summary>
        /// <param name="position">The position of the transform.</param>
        /// <param name="size">The size of the transform.</param>
        protected TransformRenderable(Vector2 position, Vector2 size) : base(position.X, position.Y, 0, size.X, size.Y)
        {
        }

        /// <summary>
        /// Create a new renderable transform from a vec3 and a vec2.
        /// </summary>
        /// <param name="position">The position of the transform.</param>
        /// <param name="size">The size of the transform.</param>
        protected TransformRenderable(Vector3 position, Vector2 size) : base(position.X, position.Y, position.Z, size.X, size.Y)
        {
        }

        /// <summary>
        /// Create a new transform;
        /// </summary>
        /// <param name="x">The position of the transform on the X axis.</param>
        /// <param name="y">The position of the transform on the Y axis.</param>
        /// <param name="z">The position of the transform ont he Z axis.</param>
        /// <param name="width">The width of the transform.</param>
        /// <param name="height">The height of the transform.</param>
        protected TransformRenderable(float x = 0f, float y = 0f, float z = 0f, float width = 0f, float height = 0f) : base(x, y, z, width, height)
        {
        }

        #endregion

        /// <summary>
        /// Render the renderable.
        /// </summary>
        public abstract void Render(RenderComposer composer);
    }
}