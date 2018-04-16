// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine.Objects;
using Emotion.Platform.SDL2;
using Emotion.Platform.SDL2.Assets;
using Emotion.Platform.SDL2.Objects;
using Emotion.Primitives;

#endregion

namespace EmotionSandbox.Examples
{
    public class RenderingText : Layer
    {
        private static SDLContext _context;
        private SDLFont _font;
        private SDLTexture _cachedTextRender;

        public static void Main()
        {
            _context = new SDLContext();

            _context.LayerManager.Add(new RenderingText(), "Text Example", 0);
            _context.Start();
        }

        public override void Load()
        {
            _font = _context.AssetLoader.LoadFont("ElectricSleepFont.ttf");
        }

        public override void Draw()
        {
            // Show direct SDL ttf rendering.
            _context.Renderer.DrawText(_font, "Hello! I am text being rendered using SDL.TTF.".ToUpper(), Color.White, new Vector2(50, 50), 40);

            // Check if need to render.
            if (_cachedTextRender == null)
            {
                // Render.
                TextDrawingSession session = _context.Renderer.TextSessionStart(_font, 40, 800, 300);
                foreach (char c in "Hello! I am text being rendered using a text session!".ToUpper())
                {
                    _context.Renderer.TextSessionAddGlyph(session, c, Color.White);
                }
                _cachedTextRender = _context.Renderer.TextSessionEnd(session, true);
            }

            // Render cached.
            _context.Renderer.DrawTexture(_cachedTextRender,
                new Rectangle(50, 100, _cachedTextRender.Width, _cachedTextRender.Height));
        }

        public override void Update()
        {
        }

        public override void Unload()
        {
        }
    }
}