// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine.Objects;
using Emotion.Platform.SDL2;
using Emotion.Platform.SDL2.Assets;
using Emotion.Primitives;

#endregion

namespace EmotionSandbox.Examples
{
    public class RenderingTexture : Layer
    {
        private static Context _context;
        private static Texture _texture;

        public static void Main()
        {
            _context = new Context
            {
                AssetLoader =
                {
                    RootDirectory = "Assets"
                }
            };


            _context.Start();
        }

        public override void Load()
        {
            _texture = _context.AssetLoader.LoadTexture("test.png");
        }

        public override void Draw()
        {
            _context.Renderer.DrawTexture(_texture, new Rectangle(0, 0, 100, 100), new Rectangle(0, 0, 200, 200));
        }

        public override void Update()
        {
            
        }

        public override void Unload()
        {
            
        }
    }
}