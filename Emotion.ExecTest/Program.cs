#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.Text;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
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
            Engine.Setup(new Configurator().SetDebug(true).AddPlugin(new ImGuiNetPlugin()));
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
        }

        public void Load()
        {

        }

        public void Unload()
        {

        }
    }
}