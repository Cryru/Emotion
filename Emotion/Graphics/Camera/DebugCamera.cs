#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Platform.Input;

#endregion

namespace Emotion.Graphics.Camera
{
    public class DebugCamera : PixelArtCamera
    {
        public float Speed = 0.35f;

        public DebugCamera(Vector3 position, float zoom = 1) : base(position, zoom)
        {
            Engine.InputManager.OnMouseScroll.AddListener(OnMouseScroll);
        }

        private bool OnMouseScroll(float val)
        {
            Zoom += 0.25f * val;
            Zoom = Utility.Maths.Clamp(Zoom, 0.25f, 4);
            RecreateMatrix();
            return true;
        }

        public override void Dispose()
        {
            Engine.InputManager.OnMouseScroll.RemoveListener(OnMouseScroll);
            base.Dispose();
        }

        public override void Update()
        {
            Vector2 dir = Vector2.Zero;
            if (Engine.InputManager.IsKeyHeld(Key.Kp8)) dir.Y -= 1;
            if (Engine.InputManager.IsKeyHeld(Key.Kp4)) dir.X -= 1;
            if (Engine.InputManager.IsKeyHeld(Key.Kp2)) dir.Y += 1;
            if (Engine.InputManager.IsKeyHeld(Key.Kp6)) dir.X += 1;

            dir *= new Vector2(Speed) * Engine.DeltaTime;
            Position += new Vector3(dir, 0);
            base.Update();
        }
    }
}