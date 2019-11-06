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
    public class CameraBase : Positional
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
        public Matrix4x4 ViewMatrix { get; protected set; }

        /// <summary>
        /// The camera's matrix without scaling applied.
        /// </summary>
        public Matrix4x4 ViewMatrixUnscaled { get; protected set; }

        #endregion

        /// <summary>
        /// Create a new camera basis.
        /// </summary>
        /// <param name="position">The position of the camera.</param>
        /// <param name="size">The size of the camera's viewport.</param>
        /// <param name="zoom">The camera's zoom.</param>
        public CameraBase(Vector3 position, float zoom = 1f)
        {
            Position = position;
            Zoom = zoom;

            RecreateMatrix();

            OnMove += (s, e) => { RecreateMatrix(); };
        }

        /// <summary>
        /// Update the camera. The current camera is updated before each frame.
        /// </summary>
        public virtual void Update()
        {
        }

        /// <summary>
        /// Recreates the view matrix of the camera and updates it for the renderer.
        /// </summary>
        public virtual void RecreateMatrix()
        {
            Vector2 targetSize = Engine.Configuration.RenderSize;
            Vector2 currentSize = Engine.Renderer.DrawBuffer.Size;

            // Transform the position from the center position to the offset position.
            Vector3 posOffset = Position - new Vector3(targetSize, 0) / 2;

            // Get the scale relative to the zoom.
            float scale = Engine.Renderer.IntScale * Zoom;

            // Find the camera margin and scale from the center.
            // As the current size expands more of the world will come into view until the integer scale changes at which point everything will be resized.
            // This allows for pixel art to scale integerly.
            Vector2 margin = (currentSize - targetSize) / 2;
            Vector3 pos = posOffset - new Vector3(margin, 0);
            ViewMatrixUnscaled = Matrix4x4.CreateTranslation(-pos.X, -pos.Y, pos.Z);
            ViewMatrix = Matrix4x4.CreateScale(new Vector3(scale, scale, 1), new Vector3(X, Y, 0)) * ViewMatrixUnscaled;
        }

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
    }
}