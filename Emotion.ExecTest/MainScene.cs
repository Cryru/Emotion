// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Numerics;
using Emotion.Engine;
using Emotion.Engine.Scenography;
using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace Emotion.ExecTest
{
    public class MainScene : Scene
    {
        public static void Main(string[] args)
        {
            Context.Setup();
            Context.SceneManager.SetScene(new MainScene());
            Context.Run();
        }

        public override void Load()
        {
        }

        public override void Update(float frameTime)
        {
        }

        public override void Draw(Renderer renderer)
        {
            Context.Renderer.Render(new Vector3(10, 10, 0), new Vector2(10, 10), Color.White);
        }

        public override void Unload()
        {
        }
    }
}