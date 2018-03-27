// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Objects.Bases;
using Emotion.Platform;
using Emotion.Platform.Assets;
using Emotion.Primitives;

#endregion

namespace Emotion.Objects.Game
{
    /// <summary>
    /// A camera which centers on the target but uses input from the mouse to determine location.
    /// </summary>
    public sealed class MouseCamera : Camera
    {

        #region Properties

        /// <summary>
        /// The maximum distance the camera can be between the target and the location of the mouse. From 0 to 1, 0 being exactly at the target, and 1 being exactly at the mouse pointer.
        /// </summary>
        public float CameraMaxDistance = 0.3f;

        /// <summary>
        /// An offset of the view from the target.
        /// </summary>
        public Vector2 CameraOffset = new Vector2(0, 0);

        #endregion

        private Vector2 _targetLastPosition;

        public MouseCamera(Rectangle bounds) : base(bounds)
        {
            
        }

        public void Update(Input input)
        {
            // Check if no target.
            if (Target == null) return;

            // Check if target last position is set. It isn't on the first frame.
            if (_targetLastPosition == Vector2.Zero) _targetLastPosition = Target.Bounds.Center;

            // Get mouse location.
            Vector2 mouseLocation = input.GetMousePosition(this);

            // Smooth between the mouse location and the target.
            float lx = GameMath.Lerp(Target.Bounds.Center.X, mouseLocation.X, CameraMaxDistance);
            float ly = GameMath.Lerp(Target.Bounds.Center.Y, mouseLocation.Y, CameraMaxDistance);

            Bounds.Center = new Vector2(lx, ly);

            // Record position.
            _targetLastPosition = Target.Bounds.Center;
        }
    }
}