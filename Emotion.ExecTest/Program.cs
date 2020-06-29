#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.ExecTest.Examples;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Plugins.ImGuiNet;
using Emotion.Primitives;
using Emotion.Scenography;

#endregion

namespace Emotion.ExecTest
{
    internal class Program : IScene
    {
        private static int dirX;
        private static int dirY;
        private static readonly List<Vector2> mousePosTest = new List<Vector2>();

        private static void Main(string[] args)
        {
            var config = new Configurator
            {
                DebugMode = true,
                GlDebugMode = true
            };
            config.AddPlugin(new ImGuiNetPlugin());

            Engine.Setup(config);
            Engine.SceneManager.SetScene(new Program());
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
            Engine.Host.OnMouseKey.AddListener((key, _) =>
            {
                if (key == MouseKey.Left) mousePosTest.Add(Engine.Host.MousePosition);
                return true;
            });

            Engine.Run();
        }

        public void Update()
        {
            Engine.Renderer.Camera.X += dirX * 1 * Engine.DeltaTime;
            Engine.Renderer.Camera.Y += dirY * 1 * Engine.DeltaTime;

            Engine.Renderer.Camera.RecreateMatrix();
        }

        public void Draw(RenderComposer composer)
        {
            composer.RenderSprite(new Vector3(0, 0, 0), Engine.Renderer.CurrentTarget.Size, Color.CornflowerBlue);
            composer.RenderSprite(new Vector3(0, 0, 0), new Vector2(10, 10), Color.Red);
            composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size - new Vector2(10, 10), 0), new Vector2(10, 10), Color.Red);

            const int count = ushort.MaxValue * 2;
            const int size = 1;

            var y = 0;
            var x = 0;
            var elements = 0;

            while (elements < count)
            {
                var c = new Color(elements, 255 - elements, elements < ushort.MaxValue ? 255 : 0);

                composer.RenderSprite(new Vector3(x * size, y * size, 0), new Vector2(size, size), c);
                x++;
                elements++;

                if (x * size < Engine.Renderer.CurrentTarget.Size.X) continue;
                y++;
                x = 0;
            }
        }

        public void Load()
        {
        }

        public void Unload()
        {
        }
    }
}