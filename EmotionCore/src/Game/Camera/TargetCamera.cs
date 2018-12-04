// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Numerics;
using Emotion.Engine;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.Camera
{
    /// <summary>
    /// A camera which follows the target closely.
    /// </summary>
    public class TargetCamera : CameraBase
    {
        /// <summary>
        /// The object to follow.
        /// </summary>
        public Transform Target { get; set; }

        /// <summary>
        /// The speed at which the camera should move. From 0 to 1, 0 being an immovable camera, and 1 being always at the target.
        /// </summary>
        public float Speed = 0.1f;

        public TargetCamera(Vector3 position, Vector2 size) : base(position, size)
        {
        }

        public override void Update()
        {
            // Check if no target.
            if (Target == null) return;

            // Smooth.
            float lx = MathExtension.Lerp(Center.X, Target.X, MathExtension.Clamp(Speed * Context.FrameTime, 0, 1));
            float ly = MathExtension.Lerp(Center.Y, Target.Y, MathExtension.Clamp(Speed * Context.FrameTime, 0, 1));

            Center = new Vector2(lx, ly);

            UpdateMatrix();
        }
    }
}