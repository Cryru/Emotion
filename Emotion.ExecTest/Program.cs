#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Scenography;

#endregion

namespace Emotion.ExecTest
{
    public class Program : IScene
    {
        private static int _dirX;
        private static int _dirY;
        private static readonly List<Vector2> MousePosTest = new List<Vector2>();

        private static void Main()
        {
            var config = new Configurator
            {
                DebugMode = true
            };

            Engine.Setup(config);
            Engine.SceneManager.SetScene(new Program());
            Engine.Run();
        }

        public void Update()
        {
            Engine.Renderer.Camera.X += _dirX * 1 * Engine.DeltaTime;
            Engine.Renderer.Camera.Y += _dirY * 1 * Engine.DeltaTime;
        }

        public void Draw(RenderComposer composer)
        {
            
            //composer.RenderSprite(new Vector3(0, 0, 0), new Vector2(10, 10), Color.Red);
            //composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size - new Vector2(10, 10), 0), new Vector2(10, 10), Color.Red);

            //for (var i = 0; i < MousePosTest.Count; i++)
            //{
            //    composer.RenderSprite(MousePosTest[i].ToVec3(), new Vector2(10), Color.Magenta);
            //}


            composer.SetUseViewMatrix(false);
            //var font = Engine.AssetLoader.Get<FontAsset>("CaslonOS.otf");
            //var font = Engine.AssetLoader.Get<FontAsset>("Junction-bold.otf");
            //var font = Engine.AssetLoader.Get<FontAsset>("Ubuntu/Ubuntu-Regular.ttf");
            var font = Engine.AssetLoader.Get<FontAsset>("TIMES.ttf");
            composer.RenderSprite(new Vector3(0, 0, 0), Engine.Renderer.CurrentTarget.Size, new Color(229, 229, 229));

            composer.PushModelMatrix(Matrix4x4.CreateTranslation(100, 100, 0));

            Color textCol = new Color(26, 26, 26);
            composer.RenderStringTest(new Vector3(0, -20, 0), textCol, "The quick brown fox did a thing.", font.GetAtlas(8));
            composer.RenderStringTest(Vector3.Zero, textCol, "The quick brown fox did a thing.", font.GetAtlas(10));
            composer.RenderStringTest(new Vector3(0, 25, 0), textCol, "The quick brown fox did a thing.", font.GetAtlas(15));
            composer.RenderStringTest(new Vector3(0, 55, 0), textCol, "The quick brown fox did a thing.", font.GetAtlas(20));
            composer.RenderStringTest(new Vector3(0, 85, 0), textCol, "The quick brown fox did a thing.", font.GetAtlas(30));
            composer.RenderStringTest(new Vector3(0, 135, 0), textCol, "The quick brown fox did a thing.", font.GetAtlas(40));
            composer.RenderStringTest(new Vector3(0, 185, 0), textCol, "The quick brown fox did a thing.", font.GetAtlas(80));
            //composer.RenderSprite(Vector3.Zero, new Vector2(50, 50), Color.Red);
            //composer.RenderStringTest(Vector3.Zero, textCol, "The quick brown fox did a thing.", font.GetAtlas(30));
            composer.PopModelMatrix();

            composer.SetUseViewMatrix(true);
            //composer.RenderString(Vector3.Zero, Color.White, "The quick brown fox did a thing.", font.GetAtlas(10));
            //composer.RenderString(new Vector3(0, 11, 0), Color.White, "The quick brown fox did a thing.", font.GetAtlas(15));
            //composer.RenderString(new Vector3(0, 25, 0), Color.White, "The quick brown fox did a thing.", font.GetAtlas(20));

            //composer.RenderString(Vector3.Zero, Color.White, "The quick brown fox did a thing.", font.GetAtlas(200));
            //composer.RenderString(new Vector3(0, 11, 0), Color.White, "The quick brown fox did a thing.", font.GetAtlas(15));
            //composer.RenderString(new Vector3(0, 25, 0), Color.White, "The quick brown fox did a thing.", font.GetAtlas(20));

            var sdf = TestGlyphRenderer.LastProducedSdf;
            if (sdf != null)
            {
                composer.RenderSprite(new Vector3(0,500, 0), sdf.Size, sdf.ColorAttachment);
            }
        }

        public void Load()
        {
            Engine.Host.OnKey.AddListener((key, status) =>
            {
                if (key == Key.W)
                {
                    if (status == KeyStatus.Down)
                        _dirY -= 1;
                    else if (status == KeyStatus.Up)
                        _dirY += 1;
                }
                else if (key == Key.S)
                {
                    if (status == KeyStatus.Down)
                        _dirY += 1;
                    else if (status == KeyStatus.Up)
                        _dirY -= 1;
                }
                else if (key == Key.A)
                {
                    if (status == KeyStatus.Down)
                        _dirX -= 1;
                    else if (status == KeyStatus.Up)
                        _dirX += 1;
                }
                else if (key == Key.D)
                {
                    if (status == KeyStatus.Down)
                        _dirX += 1;
                    else if (status == KeyStatus.Up)
                        _dirX -= 1;
                }
                else if (key == Key.MouseKeyLeft)
                {
                    MousePosTest.Add(Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition));
                }

                return true;
            });
        }

        public void Unload()
        {
        }
    }
}