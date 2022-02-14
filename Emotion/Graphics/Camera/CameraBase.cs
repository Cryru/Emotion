#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Primitives;

#endregion

namespace Emotion.Graphics.Camera
{
    /// <summary>
    /// The basis for a camera object.
    /// </summary>
    public abstract class CameraBase : Positional, IDisposable
    {
        #region Properties

        /// <summary>
        /// The FarZ clipping plane. All Z vertices past this wont be rendered.
        /// </summary>
        public float FarZ
        {
            get => _farZ;
            set
            {
                _farZ = value;
                RecreateProjectionMatrix();
            }
        }

        private float _farZ = 100;

        /// <summary>
        /// The NearZ clipping plane. All Z vertices before this wont be rendered.
        /// </summary>
        public float NearZ
        {
            get => _nearZ;
            set
            {
                _nearZ = value;
                RecreateProjectionMatrix();
            }
        }

        private float _nearZ = -100;

        /// <summary>
        /// The direction (normalized) vector the camera is looking in.
        /// </summary>
        public Vector3 LookAt
        {
            get => _lookAt;
            set
            {
                LookAtChanged(_lookAt, value);
                _lookAt = value;
                RecreateViewMatrix();
            }
        }

        protected Vector3 _lookAt = new Vector3(0, 0, -1);

        /// <summary>
        /// Calculated camera scale from the zoom and render size scale.
        /// Is applied to the view matrix in 2d cameras.
        /// </summary>
        public float CalculatedScale { get; protected set; } = 1;

        /// <summary>
        /// How zoomed the camera is.
        /// </summary>
        public float Zoom
        {
            get => _zoom;
            set
            {
                _zoom = value;
                RecreateViewMatrix();
            }
        }

        protected float _zoom;

        #endregion

        #region Calculated

        /// <summary>
        /// The camera's matrix.
        /// </summary>
        public Matrix4x4 ViewMatrix
        {
            get => _viewMatrix;
            set
            {
                _viewMatrix = value;
                SyncRenderer();
            }
        }

        /// <summary>
        /// The camera's projection matrix.
        /// </summary>
        public Matrix4x4 ProjectionMatrix
        {
            get => _projectionMatrix;
            set
            {
                _projectionMatrix = value;
                SyncRenderer();
            }
        }

        private Matrix4x4 _viewMatrix = Matrix4x4.Identity;
        private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;

        #endregion

        /// <summary>
        /// Create a new camera basis.
        /// </summary>
        /// <param name="position">The position of the camera.</param>
        /// <param name="zoom">The camera's zoom.</param>
        protected CameraBase(Vector3 position, float zoom = 1f)
        {
            Position = position;
            _zoom = zoom;
            LookAtChanged(Vector3.Zero, _lookAt);
            RecreateViewMatrix();
            RecreateProjectionMatrix();
            OnMove += (s, e) => { RecreateViewMatrix(); };
        }

        /// <summary>
        /// Update the camera. The current camera is updated each tick.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Recreates the view matrix of the camera.
        /// </summary>
        public abstract void RecreateViewMatrix();

        /// <summary>
        /// Recreates the projection matrix of the camera.
        /// </summary>
        public virtual void RecreateProjectionMatrix()
        {
            ProjectionMatrix = GetDefault2DProjection(NearZ, FarZ);
        }

        /// <summary>
        /// Notify the renderer that the matrix has changed, causing a state change.
        /// </summary>
        protected void SyncRenderer()
        {
            if (!Engine.Renderer.InFrame || Engine.Renderer.Camera != this || !GLThread.IsGLThread()) return;
            Engine.Renderer.FlushRenderStream();
            Engine.Renderer.SyncShader();
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

        /// <summary>
        /// Return a Rectangle which bounds the visible section of the game world.
        /// </summary>
        /// <returns>Rectangle bounding the visible section of the world.</returns>
        public Rectangle GetWorldBoundingRect()
        {
            Vector2 start = ScreenToWorld(Vector2.Zero);
            return new Rectangle(
                start,
                ScreenToWorld(Engine.Renderer.DrawBuffer.Size) - start
            );
        }

        #region Events

        protected virtual void LookAtChanged(Vector3 oldVal, Vector3 newVal)
        {
        }

        #endregion

        /// <summary>
        /// Get the default 2d projection matrix for the currently bound framebuffer.
        /// </summary>
        /// <returns></returns>
        public static Matrix4x4 GetDefault2DProjection(float nearZ = -100, float farZ = 100)
        {
            RenderComposer renderer = Engine.Renderer;
            return Matrix4x4.CreateOrthographicOffCenter(0, renderer.CurrentTarget.Size.X, renderer.CurrentTarget.Size.Y, 0, nearZ, farZ);
        }

        /// <summary>
        /// Destroy the camera and any internal resources it owns.
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
}