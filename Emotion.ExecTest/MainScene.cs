// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine;
using Emotion.Engine.Scenography;
using Emotion.Graphics;

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
        }

        public override void Unload()
        {
        }
    }
}