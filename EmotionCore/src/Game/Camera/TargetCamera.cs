// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine;
using Emotion.Primitives;
using Soul;

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

        public TargetCamera(Rectangle bounds) : base(bounds)
        {
        }

        public override void Update(Context context)
        {
            // Check if no target.
            if (Target == null) return;

            // Smooth.
            float lx = MathHelper.Lerp(Bounds.Center.X, Target.Bounds.X, MathHelper.Clamp(Speed * context.FrameTime, 0, 1));
            float ly = MathHelper.Lerp(Bounds.Center.Y, Target.Bounds.Y, MathHelper.Clamp(Speed * context.FrameTime, 0, 1));

            Center = new Vector2(lx, ly);

            if (!_transformUpdated) return;
            ViewMatrix = Matrix4.CreateTranslation(X, Y, Z);
            _transformUpdated = false;
        }
    }
}