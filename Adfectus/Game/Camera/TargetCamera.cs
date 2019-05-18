#region Using

using System;
using System.Numerics;
using Adfectus.Common;
using Adfectus.Logging;
using Adfectus.Primitives;
using Adfectus.Utility;

#endregion

namespace Adfectus.Game.Camera
{
    /// <summary>
    /// A camera which follows the target closely.
    /// </summary>
    public class TargetCamera : CameraBase
    {
        #region Properties

        /// <summary>
        /// The object to follow.
        /// </summary>
        public Transform Target
        {
            get => _target;
            set
            {
                // Detach from old target events - if any.
                if (_target != null)
                {
                    _target.OnMove += TargetEventTracker;
                    _target.OnResize -= TargetEventTracker;
                }

                // Set target to value.
                _target = value;

                // Attach to events.
                _target.OnMove += TargetEventTracker;
                _target.OnResize += TargetEventTracker;
            }
        }

        /// <summary>
        /// The speed at which the camera should move. From 0 to 1, 0 being an immovable camera, and 1 being always at the target.
        /// </summary>
        public float Speed = 0.1f;

        #endregion

        /// <summary>
        /// A private tracker for the target.
        /// </summary>
        private Transform _target;

        /// <summary>
        /// Create a new camera which follows a target transform.
        /// </summary>
        /// <param name="position">The starting position of the camera.</param>
        /// <param name="size">The size of the camera's viewport.</param>
        /// <param name="zoom">The camera's zoom.</param>
        public TargetCamera(Vector3 position, Vector2 size, float zoom = 1f) : base(position, size, zoom)
        {
        }

        /// <summary>
        /// The camera is updated once per frame automatically by the renderer.
        /// </summary>
        public override void Update()
        {
            // Check if no target.
            if (Target == null) return;

            // Smooth.
            float lx = MathExtension.Lerp(Center.X, Target.X, MathExtension.Clamp(Speed * Engine.FrameTime, 0, 1));
            float ly = MathExtension.Lerp(Center.Y, Target.Y, MathExtension.Clamp(Speed * Engine.FrameTime, 0, 1));

            Center = new Vector2(lx, ly);
        }

        /// <summary>
        /// Used to track target movement. Ensures the camera is up to date.
        /// </summary>
        /// <param name="sender">The target itself.</param>
        /// <param name="e">Empty.</param>
        private void TargetEventTracker(object sender, EventArgs e)
        {
            if (sender != Target) Engine.Log.Warning("Received a camera update event from an unknown source.", MessageSource.Game);

            Update();
        }
    }
}