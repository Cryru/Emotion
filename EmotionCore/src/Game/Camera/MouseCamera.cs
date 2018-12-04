// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Numerics;
using Emotion.Engine;

#endregion

namespace Emotion.Game.Camera
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

        private Vector2 _targetLastPosition;

        public MouseCamera(Vector3 position, Vector2 size) : base(position, size)
        {
        }

        public override void Update()
        {
            // Check if no target.
            if (Target == null) return;

            // Check if target last position is set. It isn't on the first frame.
            if (_targetLastPosition == Vector2.Zero) _targetLastPosition = Target.Center;

            // Get mouse location.
            Vector2 mouseLocation = Context.Renderer.ScreenToWorld(Context.InputManager.GetMousePosition());

            // Smooth between the mouse location and the target.
            float lx = MathExtension.Lerp(Target.Center.X, mouseLocation.X, MathExtension.Clamp(Speed * Context.FrameTime, 0, CameraMaxDistance));
            float ly = MathExtension.Lerp(Target.Center.Y, mouseLocation.Y, MathExtension.Clamp(Speed * Context.FrameTime, 0, CameraMaxDistance));

            Center = new Vector2(lx, ly);

            // Record position.
            _targetLastPosition = Target.Center;

            UpdateMatrix();
        }
    }
}