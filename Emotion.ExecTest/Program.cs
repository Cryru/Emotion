#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.ExecTest.Examples;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Scenography;

#endregion

namespace Emotion.ExecTest
{
    public class Program : IScene
    {
        private static void Main()
        {
            var config = new Configurator
            {
                DebugMode = true
            };

            Engine.Setup(config);
            Engine.SceneManager.SetScene(new TextRenderers());
            Engine.Run();
        }

        public void Load()
        {
        }

        public void Update()
        {
        }

        public void Draw(RenderComposer composer)
        {
            composer.SetUseViewMatrix(false);
            composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);
        }

        public void Unload()
        {
        }
    }
}