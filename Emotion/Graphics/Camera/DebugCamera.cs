#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Platform.Input;
using Emotion.Utility;

#endregion

namespace Emotion.Graphics.Camera
{
    public class DebugCamera : PixelArtCamera
    {
        public float Speed = 0.35f;

        public DebugCamera(Vector3 position, float zoom = 1) : base(position, zoom)
        {
            Engine.Host.OnMouseScroll += OnMouseScroll;
        }

        private void OnMouseScroll(float val)
        {
            Zoom = Maths.Clamp(Zoom + 0.25f * val, 0.25f, 4);
        }

        public override void Dispose()
        {
            Engine.Host.OnMouseScroll -= OnMouseScroll;
            base.Dispose();
        }

        public override void Update()
        {
            Vector2 dir = Vector2.Zero;
            if (Engine.Host.IsKeyHeld(Key.Kp8)) dir.Y -= 1;
            if (Engine.Host.IsKeyHeld(Key.Kp4)) dir.X -= 1;
            if (Engine.Host.IsKeyHeld(Key.Kp2)) dir.Y += 1;
            if (Engine.Host.IsKeyHeld(Key.Kp6)) dir.X += 1;

            dir *= new Vector2(Speed) * Engine.DeltaTime;
            Position += new Vector3(dir, 0);
            base.Update();
        }
    }
}