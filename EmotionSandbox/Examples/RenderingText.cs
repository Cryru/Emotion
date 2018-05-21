// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion;
using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.GLES;
using Emotion.GLES.Text;
using Emotion.IO;
using Emotion.Primitives;

#endregion

namespace EmotionSandbox.Examples
{
    public class RenderingText : Layer
    {
        private Font _font;
        private ITexture _cachedTextRender;

        public static void Main()
        {
            Context context = Starter.GetEmotionContext();

            context.LayerManager.Add(new LoadingScreen(), "loading", -1);
            context.LayerManager.Add(new RenderingText(), "Text Example", 0);

            context.Start();
        }

        public override void Load()
        {
            _font = Context.AssetLoader.Get<Font>("ElectricSleepFont.ttf");
        }

        private uint demoFontSize = 32;

        public override void Draw(Renderer renderer)
        {
            renderer.DrawRectangle(new Rectangle(0, 0, renderer.RenderSize.X, renderer.RenderSize.Y), Color.CornflowerBlue, false);

            // Direct context rendering.
            renderer.DrawText(_font, demoFontSize, "Hello! I am text being rendered using the Renderer.DrawText function!".ToUpper(), Color.White, new Vector2(50, 50));

            // Check if need to render.
            if (_cachedTextRender == null)
            {
                // Render.
                TextDrawingSession session = renderer.StartTextSession(_font, demoFontSize, 800, 300);
                foreach (char c in "Hello! I am text being rendered using a text session!".ToUpper())
                {
                    session.AddGlyph(c, Color.White);
                }

                _cachedTextRender = session.GetTexture();
                session.Destroy();
            }

            // Render cached.
            renderer.DrawTexture(_cachedTextRender,
                new Rectangle(50, 100, _cachedTextRender.Width, _cachedTextRender.Height));
        }

        public override void Update(float frameTime)
        {
        }

        public override void Unload()
        {
        }
    }
}