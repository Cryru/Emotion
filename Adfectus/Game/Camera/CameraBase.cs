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
            ViewMatrix = Matrix4x4.CreateTranslation(Width / 2, Height / 2, Z).Inverted() * Matrix4x4.CreateScale(Zoom) * Matrix4x4.CreateTranslation(Width / 2, Height / 2, Z) *
                         Matrix4x4.CreateTranslation(-X, -Y, Z);

            Engine.Renderer?.UpdateCameraMatrix();
        }
    }
}