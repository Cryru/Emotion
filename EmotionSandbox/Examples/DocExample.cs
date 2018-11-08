// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.Graphics;

#endregion

namespace MyEmotionProject
{
    internal class Program
    {
        public static void Main()
        {
            Context.Setup();
        }
    }

    internal class MyLayer : Layer
    {
        public override void Load()
        {
        }

        public override void Draw(Renderer renderer)
        {
        }

        public override void Update(float frameTime)
        {
        }

        public override void Unload()
        {
        }
    }
}