// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.IO;
using Emotion.Platform;
using Emotion.Platform.Assets;
using Emotion.Primitives;

#endregion

namespace EmotionSandbox.Examples
{
    public class RenderingText
    {
        private static Context _context;
        private static Font _font;

        public static void Main()
        {
            _context = new Context();

            _context.AssetLoader.RootDirectory = "Assets";
            _font = _context.AssetLoader.LoadFont("ElectricSleepFont.ttf");

            _context.Start(Draw);
        }

        private static void Draw()
        {
            _context.Renderer.DrawText(_font, "Hello sir! This is a text rendering demo.".ToUpper(), Color.White, new Vector2(0, 0), 40);
        }
    }
}