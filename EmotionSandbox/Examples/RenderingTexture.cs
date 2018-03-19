// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Drawing;
using System.IO;
using Emotion.Assets;
using Emotion.Engine;

#endregion

namespace EmotionSandbox.Examples
{
    public class RenderingTexture
    {
        private static Context _context;
        private static Texture _texture;

        public static void Main()
        {
            _context = new Context();

            byte[] data = File.ReadAllBytes("test.png");
            _texture = new Texture(_context, data);

            _context.Start(Draw);
        }

        private static void Draw()
        {
            _context.Renderer.DrawTexture(_texture, new Rectangle(0, 0, 100, 100), new Rectangle(0, 0, 100, 100));
        }
    }
}