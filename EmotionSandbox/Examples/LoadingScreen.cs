// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Game.Layering;
using Emotion.GLES;
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
            renderer.DrawRectangle(new Rectangle(_x, _y, 10, 10), Color.Red, false);

            _x += 10;

            if (_x >= renderer.RenderSize.X)
            {
                _x = 0;
                _y += 10;
            }

            if (_y >= renderer.RenderSize.Y)
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
            Context.AssetLoader.Free("test.png");
        }
    }
}