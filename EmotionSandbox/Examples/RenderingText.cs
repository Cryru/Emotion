// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine.Objects;
using Emotion.Platform.SDL2;
using Emotion.Platform.SDL2.Assets;
using Emotion.Primitives;

#endregion

namespace EmotionSandbox.Examples
{
    public class RenderingText : Layer
    {
        private static Context _context;
        private Font _font;
        private Texture _cachedTextRender;

        public static void Main()
        {
            _context = new Context();

            _context.LayerManager.Add(new RenderingText(), "Text Example", 0);
            _context.Start();
        }

        public override void Load()
        {
            _font = _context.AssetLoader.LoadFont("ElectricSleepFont.ttf");
        }

        public override void Draw()
        {
            string text = ".".ToUpper();

            _context.Renderer.DrawText(_font, text, Color.White, new Vector2(0, 0), 40);
            if (_cachedTextRender == null)
            {
                _context.Renderer.TextSessionStart(_font, 40, 550, 300);
                for (int i = 0; i < text.Length; i++)
                {
                    _context.Renderer.TextSessionAddGlyph(text[i], Color.White, 0, 0);
                }
                _cachedTextRender = _context.Renderer.TextSessionEnd();
            }

            _context.Renderer.DrawTexture(_cachedTextRender,
                new Rectangle(0, 0, _cachedTextRender.Width, _cachedTextRender.Height));
        }

        public override void Update()
        {
        }

        public override void Unload()
        {
        }
    }
}