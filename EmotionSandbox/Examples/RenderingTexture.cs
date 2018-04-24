// Emotion - https://github.com/Cryru/Emotion

#region Using

#if GLES
using Emotion.Platform.GLES;
#endif
using Emotion.Engine.Objects;
using Emotion.Platform.Base.Assets;
using Emotion.Primitives;
#if SDL2
using Emotion.Platform.SDL2;
#endif

#endregion

namespace EmotionSandbox.Examples
{
    public class RenderingTexture : Layer
    {
#if GLES
        private static GLContext _context;
#endif
#if SDL2
        private static SDLContext _context;
#endif

        private Texture _texture;

        public static void Main()
        {
#if GLES
            _context = new GLContext();
#endif
#if SDL2
            _context = new SDLContext();
#endif

            _context.LayerManager.Add(new RenderingTexture(), "Texture Example", 0);
            _context.Start();
        }

        public override void Load()
        {
            _texture = _context.AssetLoader.Texture("test.png");
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