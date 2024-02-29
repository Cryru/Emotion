#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.Utility;

#endregion

namespace Emotion.ExecTest.Examples
{
    public class PngSuiteExample : IScene
    {
        public void Load()
        {
        }

        public void Update()
        {
            float speed = 0.35f;

            Vector2 dir = Vector2.Zero;
            if (Engine.Host.IsKeyHeld(Key.W)) dir.Y -= 1;
            if (Engine.Host.IsKeyHeld(Key.A)) dir.X -= 1;
            if (Engine.Host.IsKeyHeld(Key.S)) dir.Y += 1;
            if (Engine.Host.IsKeyHeld(Key.D)) dir.X += 1;

            if (Engine.Host.IsKeyHeld(Key.LeftShift)) speed *= 2;

            dir *= new Vector2(speed, speed) * Engine.DeltaTime;
            Engine.Renderer.Camera.Position += new Vector3(dir, 0);

            float zoomDir = -Engine.Host.GetMouseScrollRelative() * 0.5f;
            float zoom = Engine.Renderer.Camera.Zoom;
            zoom += speed * zoomDir;
            Engine.Renderer.Camera.Zoom = Maths.Clamp(zoom, 0.1f, 4f);
        }

        public void Draw(RenderComposer composer)
        {
            Vector2 pen = Vector2.Zero;
            string[] assets = Engine.AssetLoader.AllAssets;
            for (int i = 0; i < assets.Length; i++)
            {
                if (!assets[i].Contains(".png")) continue;

                var texture = Engine.AssetLoader.Get<TextureAsset>(assets[i]);
                if (texture == null || texture.Texture == null) continue;
                composer.RenderSprite(pen.ToVec3(), texture.Texture.Size, Color.White, texture.Texture);
                pen.X += texture.Texture.Size.X;
                if (pen.X > 1000)
                {
                    pen.Y += 500;
                    pen.X = 0;
                }
            }
        }

        public void Unload()
        {
        }
    }
}