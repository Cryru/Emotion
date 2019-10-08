#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.Text;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Primitives;

#endregion

namespace Emotion.ExecTest
{
    internal class Program
    {
        private static int dirX;
        private static int dirY;
        private static List<Vector2> mousePosTest = new List<Vector2>();
        private static RichText _rText;

        private static void Main(string[] args)
        {
            Engine.Setup(new Configurator().SetDebug(true).SetRenderSize(integerScale: true));
            Engine.DebugDrawAction = DebugDrawAction;
            Engine.DebugUpdateAction = DebugUpdateAction;
            Engine.Host.OnKey.AddListener((key, status) =>
            {
                if (key == Key.W)
                {
                    if (status == KeyStatus.Down)
                        dirY -= 1;
                    else if (status == KeyStatus.Up)
                        dirY += 1;
                }

                if (key == Key.S)
                {
                    if (status == KeyStatus.Down)
                        dirY += 1;
                    else if (status == KeyStatus.Up)
                        dirY -= 1;
                }

                if (key == Key.A)
                {
                    if (status == KeyStatus.Down)
                        dirX -= 1;
                    else if (status == KeyStatus.Up)
                        dirX += 1;
                }

                if (key == Key.D)
                {
                    if (status == KeyStatus.Down)
                        dirX += 1;
                    else if (status == KeyStatus.Up)
                        dirX -= 1;
                }

                return true;
            });
            Engine.Host.OnMouseKey.AddListener(key =>
            {
                if (key == MouseKey.Left) mousePosTest.Add(Engine.Host.MousePosition);
            });
            Engine.Run();
        }

        private static Vector3[] vertices =
        {
            new Vector3(0, 0, 0),
            new Vector3(100, 10, 0),
            new Vector3(100, 100, 0)
        };

        private static bool init = true;

        private static void DebugUpdateAction()
        {
            if (init)
            {
                DebugInit();
                init = false;
            }

            Engine.Renderer.Camera.X += dirX * 1 * Engine.DeltaTime;
            Engine.Renderer.Camera.Y += dirY * 1 * Engine.DeltaTime;

            Engine.Renderer.Camera.RecreateMatrix();
        }

        private static void DebugInit()
        {
           // _rText = new RichText(new Vector3(10, 10, 0), new Vector2(100, 50), Engine.AssetLoader.Get<FontAsset>("1980XX.ttf").GetAtlas(20));
           // _rText.Alignment = TextAlignment.Centered;
           // _rText.Text = "The quick brown fox, <color=1-50-1>jumped</> over the\nlazy dog!";
        }

        private static void DebugDrawAction(RenderComposer composer)
        {
           // composer.RenderSprite(new Vector3(0, 0, 0), Engine.Configuration.RenderSize, Color.CornflowerBlue);
            
            composer.RenderSprite(new Vector3(0, 0, 0), Engine.Renderer.CurrentTarget.Size, Color.CornflowerBlue);
            composer.RenderSprite(new Vector3(0, 0, 0), new Vector2(10, 10), Color.Red);
            composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size - new Vector2(10, 10), 0), new Vector2(10, 10), Color.Red);
            //composer.RenderSprite(new Vector3(0, 0, 0), Engine.AssetLoader.Get<TextureAsset>("test.png").Texture.Size, Color.White, Engine.AssetLoader.Get<TextureAsset>("test.png").Texture);
            //composer.PushModelMatrix(Matrix4x4.CreateTranslation(new Vector3(100, 100, 0)));
            //composer.RenderVertices(vertices, Color.White);
            //composer.PopModelMatrix();
            //composer.RenderSprite(new Vector3(-10, -10, 0), new Vector2(10, 10), Color.Red);
            //composer.RenderSprite(new Vector3(0, 0, 0), new Vector2(3, 3), Color.Pink);
            //var asset = Engine.AssetLoader.Get<TextureAsset>("spritesheetAnimation.png");
            //composer.RenderSprite(new Vector3(0, 0, 0), asset.Texture.Size, Color.White, asset.Texture);
            //composer.RenderSprite(new Vector3(Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition), 0), new Vector2(3, 3), Color.Red);
            //for (int i = 0; i < mousePosTest.Count; i++)
            //{
            //    composer.RenderSprite(new Vector3(mousePosTest[i], 0), new Vector2(3, 3), Color.Pink);
            //}

            //_rText?.Render(composer);
        }
    }
}