#region Using

using System.Numerics;
using Adfectus.Common;
using Adfectus.Utility;

#endregion

namespace Adfectus.Game.Camera
{
    /// <summary>
    /// A camera which centers on the target but uses input from the mouse to determine location.
    /// </summary>
    public sealed class MouseCamera : TargetCamera
    {
        #region Properties

        /// <summary>
        /// The maximum distance the camera can be between the target and the location of the mouse. From 0 to 1, 0 being exactly
        /// at the target, and 1 being exactly at the mouse pointer.
        /// </summary>
        public float CameraMaxDistance = 0.3f;

        /// <summary>
        /// An offset of the view from the target.
        /// </summary>
        public Vector2 CameraOffset = new Vector2(0, 0);

        #endregion

        /// <summary>
        /// The last known position of the target.
        /// </summary>
        private Vector2 _targetLastPosition;

        /// <summary>
        /// Create a new camera which follows the mouse cursor.
        /// </summary>
        /// <param name="position">The starting position of the camera.</param>
        /// <param name="size">The size of the camera's viewport.</param>
        /// <param name="zoom">The camera's zoom.</param>
        public MouseCamera(Vector3 position, Vector2 size, float zoom = 1f) : base(position, size, zoom)
        {
        }

        /// <summary>
        /// The camera is updated once per frame automatically by the renderer.
        /// </summary>
        public override void Update()
        {
            // Check if no target.
            if (Target == null) return;

            // Check if target last position is set. It isn't on the first frame.
            if (_targetLastPosition == Vector2.Zero) _targetLastPosition = Target.Center;

            // Get mouse location.
            Vector2 mouseLocation = ScreenToWorld(Engine.InputManager.GetMousePosition());

            // Smooth between the mouse location and the target.
            float lx = MathExtension.Lerp(Target.Center.X, mouseLocation.X, MathExtension.Clamp(Speed * Engine.FrameTime, 0, CameraMaxDistance));
            float ly = MathExtension.Lerp(Target.Center.Y, mouseLocation.Y, MathExtension.Clamp(Speed * Engine.FrameTime, 0, CameraMaxDistance));

            Center = new Vector2(lx, ly);

            // Record position.
            _targetLastPosition = Target.Center;
        }
    }
}