// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Game.Layering;
using Emotion.GLES;
using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace EmotionSandbox.Examples
{
    public class LoadingScreen : Layer
    {
        private int _x;
        private int _y;

        public override void Load()
        {
        }

        public override void Draw(Renderer renderer)
        {
            renderer.Render(new Vector3(0, 0, 0), new Vector2(Context.Host.RenderSize.X, Context.Host.RenderSize.Y), Color.CornflowerBlue, null, Rectangle.Empty);
            renderer.Render(new Vector3(_x, _y, 0), new Vector2(10, 10), Color.Red, null, Rectangle.Empty);

            _x += 10;

            if (_x >= Context.Host.RenderSize.X)
            {
                _x = 0;
                _y += 10;
            }

            if (_y >= Context.Host.RenderSize.Y)
            {
                _x = 0;
                _y = 0;
            }
        }

        public override void Update(float frameTime)
        {
        }

        public override void Unload()
        {
            Context.AssetLoader.Destroy("test.png");
        }
    }
}