#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.IO;
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
            Helpers.CameraWASDUpdate();
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