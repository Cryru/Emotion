// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine.Objects;
using Emotion.Platform.Base.Assets;
using Emotion.Platform.GLES;
using Emotion.Primitives;

#endregion

namespace EmotionSandbox.Examples
{
    public class RenderingTexture : Layer
    {
        private static GLContext _context;
        private Texture _texture;

        public static void Main()
        {
            _context = new GLContext();

            _context.LayerManager.Add(new RenderingTexture(), "Texture Example", 0);
            _context.Start();
        }

        public override void Load()
        {
           // _texture = _context.AssetLoader.Texture("test.png");
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