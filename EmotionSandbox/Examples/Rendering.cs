// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Threading;
using Emotion;
using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.GLES;
using Emotion.IO;
using Emotion.Primitives;

#endregion

namespace EmotionSandbox.Examples
{
    public class Rendering : Layer
    {
        private Texture _texture;

        public static void Main()
        {
            Context context = Starter.GetEmotionContext();

            context.LayerManager.Add(new LoadingScreen(), "loading", -1);
            context.LayerManager.Add(new Rendering(), "Render Example", 0);

            context.Start();
        }

        public override void Load()
        {
            _texture = Context.AssetLoader.Get<Texture>("test.png");
            Thread.Sleep(1000);
            Context.LayerManager.Remove("loading");
        }

        public override void Draw(Renderer renderer)
        {
            renderer.DrawRectangle(new Rectangle(0, 0, renderer.RenderSize.X, renderer.RenderSize.Y), Color.CornflowerBlue, false);

            int columnLoc = 10;
            renderer.DrawRectangle(new Rectangle(10, columnLoc, 110, 110), Color.Red, false);
            columnLoc += 120;
            renderer.DrawRectangleOutline(new Rectangle(10, columnLoc, 110, 110), Color.Blue, false);
            columnLoc += 120;
            renderer.DrawTexture(_texture, new Rectangle(10, columnLoc, _texture.Width / 2, _texture.Height / 2));
            renderer.DrawTexture(_texture, new Rectangle(20 + _texture.Width / 2, columnLoc, _texture.Width / 2, _texture.Height / 2),
                new Rectangle(_texture.Width / 2, 0, _texture.Width / 2, _texture.Height / 2));
        }

        public override void Update(float frameTime)
        {
        }

        public override void Unload()
        {
        }
    }
}