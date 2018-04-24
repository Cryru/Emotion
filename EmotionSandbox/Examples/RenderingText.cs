//// Emotion - https://github.com/Cryru/Emotion

//#region Using

//using Emotion.Engine.Objects;
//using Emotion.Platform.Base.Assets;
//using Emotion.Platform.Base.Objects;
//using Emotion.Platform.SDL2;
//using Emotion.Primitives;

//#endregion

//namespace EmotionSandbox.Examples
//{
//    public class RenderingText : Layer
//    {
//        private static SDLContext _context;
//        private Font _font;
//        private Texture _cachedTextRender;

//        public static void Main()
//        {
//            _context = new SDLContext();

//            _context.LayerManager.Add(new RenderingText(), "Text Example", 0);
//            _context.Start();
//        }

//        public override void Load()
//        {
//            _font = _context.AssetLoader.Font("ElectricSleepFont.ttf");
//        }

//        public override void Draw()
//        {
//            // Show direct SDL ttf rendering.
//            _context.Renderer.DrawText(_font, 33, "Hello! I am text being 'rendered' using SDL.TTF.".ToUpper(), Color.White, new Vector2(50, 50));

//            // Check if need to render.
//            if (_cachedTextRender == null)
//            {
//                // Render.
//                TextDrawingSession session = _context.Renderer.StartTextSession(_font, 33, 800, 300);
//                foreach (char c in "Hello! I am text being 'rendered' using a text session!".ToUpper())
//                {
//                    session.AddGlyph(c, Color.White);
//                }

//                _cachedTextRender = session.GetTexture();
//                session.Destroy();
//            }

//            // Render cached.
//            _context.Renderer.DrawTexture(_cachedTextRender,
//                new Rectangle(50, 100, _cachedTextRender.Width, _cachedTextRender.Height));
//        }

//        public override void Update()
//        {
//        }

//        public override void Unload()
//        {
//        }
//    }
//}