// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Game.Layering;
using Emotion.GLES;
using Emotion.Graphics;
using Emotion.Graphics.GLES;
using Emotion.Primitives;

#endregion

namespace EmotionSandbox.Examples.Generic
{
    public class LoadingScreen : Layer
    {
        private float _deg;

        public override void Load()
        {
            Context.AssetLoader.Get<Texture>("EmotionLogo.png");
            Context.AssetLoader.Get<Texture>("LoadingCircleHalf.png");
        }

        public override void Draw(Renderer renderer)
        {
            float size = Context.Host.RenderSize.Y - 160;
            float centerX = Context.Host.RenderSize.X / 2 - size / 2;
            float centerY = Context.Host.RenderSize.Y / 2 - size / 2;
            float logoCenterX = Context.Host.RenderSize.X / 2 - size / 4;
            float logoCenterY = Context.Host.RenderSize.Y / 2 - size / 4;

            Matrix4 rotationMatrix =
                Matrix4.CreateTranslation(size / 2 , size / 2, 0).Inverted() *
                Matrix4.CreateRotationZ(Soul.Convert.DegreesToRadians((int) _deg)) *
                Matrix4.CreateTranslation(size / 2 , size / 2, 0) *
                Matrix4.CreateTranslation(centerX ,centerY, 0);;

            renderer.Render(new Vector3(0, 0, 0), new Vector2(Context.Host.RenderSize.X, Context.Host.RenderSize.Y), new Color("#35383d"), null, Rectangle.Empty);
            renderer.Render(Vector3.Zero, new Vector2(size, size), Color.White, Context.AssetLoader.Get<Texture>("LoadingCircleHalf.png"), null, rotationMatrix);
            renderer.Render(new Vector3(logoCenterX, logoCenterY, 0), new Vector2(size / 2, size / 2), Color.White, Context.AssetLoader.Get<Texture>("EmotionLogo.png"));
        }

        public override void Update(float frameTime)
        {
            _deg += 0.5f * frameTime;
            if (_deg >= 360) _deg = 0;
        }

        public override void Unload()
        {
            Context.AssetLoader.Destroy("test.png");
        }
    }
}