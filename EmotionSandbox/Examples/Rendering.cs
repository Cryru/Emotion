// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Threading;
using Emotion;
using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.GLES;
using Emotion.Graphics;
using Emotion.Graphics.GLES;
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
            renderer.Render(new Vector3(0, 0, 0), new Vector2(Context.Host.RenderSize.X, Context.Host.RenderSize.Y), Color.CornflowerBlue, null, Rectangle.Empty);

            int columnLoc = 10;
            renderer.Render(new Vector3(10, columnLoc, 0), new Vector2(110, 110), Color.Red, null, Rectangle.Empty);
            columnLoc += 120;
            renderer.Render(new Vector3(10, columnLoc, 0), new Vector2(110, 110), Color.Blue, null, Rectangle.Empty);
            columnLoc += 120;
            renderer.Render(new Vector3(10, columnLoc, 0), new Vector2(_texture.Size.X / 2, _texture.Size.Y / 2), Color.CornflowerBlue, _texture, new Rectangle(_texture.Size.X / 2, 0, _texture.Size.X / 2, _texture.Size.Y / 2));
            renderer.Render(new Vector3(0, 0, 0), new Vector2(Context.Host.RenderSize.X, Context.Host.RenderSize.Y), Color.CornflowerBlue, null, Rectangle.Empty);

            renderer.Flush();
        }

        public override void Update(float frameTime)
        {
        }

        public override void Unload()
        {
        }
    }
}