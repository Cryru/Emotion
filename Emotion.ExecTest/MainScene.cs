// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Numerics;
using System.Threading.Tasks;
using Emotion.Engine;
using Emotion.Engine.Hosting.Desktop;
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

        }

        public override void Unload()
        {
        }
    }
}