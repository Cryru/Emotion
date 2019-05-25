#region Using

using System.Numerics;
using Adfectus.Common;
using Adfectus.Primitives;
using Adfectus.Utility;

#endregion

namespace Adfectus.Game.Camera
{
    /// <summary>
    /// The basis for a camera object.
    /// </summary>
    public class CameraBase : Transform
    {
        #region Properties

        /// <summary>
        /// The camera's matrix.
        /// </summary>
        public Matrix4x4 ViewMatrix { get; protected set; }

        /// <summary>
        /// How zoomed the camera is.
        /// </summary>
        public float Zoom
        {
            get => _zoom;
            set
            {
                _zoom = value;
                RecreateMatrix();
            }
        }

        private float _zoom;

        #endregion

        /// <summary>
        /// Create a new camera basis.
        /// </summary>
        /// <param name="position">The position of the camera.</param>
        /// <param name="size">The size of the camera's viewport.</param>
        /// <param name="zoom">The camera's zoom.</param>
        public CameraBase(Vector3 position, Vector2 size, float zoom = 1f) : base(position, size)
        {
            _zoom = zoom;

            RecreateMatrix();
            OnMove += (a, b) => RecreateMatrix();
            OnResize += (a, b) => RecreateMatrix();
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
        protected virtual void RecreateMatrix()
        {
            //ViewMatrix = Matrix4x4.CreateScale(Zoom, Zoom, Zoom) * Matrix4x4.CreateTranslation(-(Center.X * Zoom - Width / 2), -(Center.Y * Zoom - Height / 2), Z);
            ViewMatrix = Matrix4x4.CreateScale(Zoom, Zoom, Zoom, new Vector3(Center, Z)) * Matrix4x4.CreateTranslation(-X, -Y, Z);

            Engine.Renderer?.UpdateCameraMatrix();
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