#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace Emotion.Graphics.Camera
{
    /// <summary>
    /// The basis for a camera object. Intended to be used for 2D environments and optimized for pixel art.
    /// Will automatically scale based on the "IntScale" property of the Renderer.
    /// </summary>
    public abstract class CameraBase : Positional
    {
        #region Properties

        /// <summary>
        /// How zoomed the camera is.
        /// </summary>
        public float Zoom;

        #endregion

        #region Calculated

        /// <summary>
        /// The camera's matrix.
        /// </summary>
        public Matrix4x4 ViewMatrix { get; protected set; } = Matrix4x4.Identity;

        /// <summary>
        /// The camera's matrix without scaling applied.
        /// </summary>
        public Matrix4x4 ViewMatrixUnscaled { get; protected set; } = Matrix4x4.Identity;

        #endregion

        /// <summary>
        /// Create a new camera basis.
        /// </summary>
        /// <param name="position">The position of the camera.</param>
        /// <param name="zoom">The camera's zoom.</param>
        protected CameraBase(Vector3 position, float zoom = 1f)
        {
            Position = position;
            Zoom = zoom;

            RecreateMatrix();

            OnMove += (s, e) => { RecreateMatrix(); };
        }

        /// <summary>
        /// Update the camera. The current camera is updated each tick.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Recreates the view matrix of the camera and updates it for the renderer.
        /// </summary>
        public abstract void RecreateMatrix();

        /// <summary>
        /// Transforms a point through the viewMatrix converting it from screen space to world space.
        /// </summary>
        /// <param name="position">The point to transform.</param>
        /// <returns>The provided point in the world.</returns>
        public virtual Vector2 ScreenToWorld(Vector2 position)
        {
            return Vector2.Transform(position, ViewMatrix.Inverted());
        }

        /// <summary>
        /// Transforms a point through the viewMatrix converting it from world space to screen space.
        /// </summary>
        /// <param name="position">The point to transform.</param>
        /// <returns>The provided point on the screen.</returns>
        public virtual Vector2 WorldToScreen(Vector2 position)
        {
            return Vector2.Transform(position, ViewMatrix);
        }

        /// <summary>
        /// Return a Rectangle which bounds the visible section of the game world.
        /// </summary>
        /// <returns>Rectangle bounding the visible section of the world.</returns>
        public Rectangle GetWorldBoundingRect()
        {
            return new Rectangle(
                Engine.Renderer.Camera.ScreenToWorld(Vector2.Zero),
                Engine.Renderer.Camera.ScreenToWorld(Engine.Renderer.DrawBuffer.Size) * 2
            );
        }
    }
}