#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.Text;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Plugins.ImGuiNet;
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
            Engine.Setup(new Configurator().SetDebug(true).SetRenderSize(integerScale: true).AddPlugin(new ImGuiNetPlugin()));
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

        }

        private static void DebugDrawAction(RenderComposer composer)
        {
            composer.RenderSprite(new Vector3(0, 0, 0), Engine.Renderer.CurrentTarget.Size, Color.CornflowerBlue);
            composer.RenderSprite(new Vector3(0, 0, 0), new Vector2(10, 10), Color.Red);
            composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size - new Vector2(10, 10), 0), new Vector2(10, 10), Color.Red);
        }
    }
}