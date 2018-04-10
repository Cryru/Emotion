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
        private static Font _font;

        public static void Main()
        {
            _context = new Context
            {
                AssetLoader =
                {
                    RootDirectory = "Assets"
                }
            };

            _context.LayerManager.Add(new RenderingText(), "Text Example", 0);
            _context.Start();
        }

        public override void Load()
        {
            _font = _context.AssetLoader.LoadFont("ElectricSleepFont.ttf");
        }

        public override void Draw()
        {
            _context.Renderer.DrawText(_font, "Hello sir! This is a text rendering demo.".ToUpper(), Color.White, new Vector2(0, 0), 40);
        }

        public override void Update()
        {
        }

        public override void Unload()
        {
        }
    }
}