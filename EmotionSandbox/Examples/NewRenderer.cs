// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine;
using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace EmotionSandbox.Examples
{
    public class NewRenderer
    {
        private static Renderer _renderer;
        private static Renderable2D _ren;

        public static void Main()
        {
            Settings a = new Settings();
            _renderer = new Renderer(a)
            {
                update = Update,
                draw = Draw
            };
            _ren = new Renderable2D(new Vector3(0, 0, 0), new Vector2(100, 100), Color.Blue, _renderer.DefaultShaderProgram);
            _renderer.Run();
        }

        public static void Draw(float fr)
        {
            _renderer.Render(_ren);
            //_renderer.Render(new Renderable2D(new Vector3(100, 100, 0), new Vector2(100, 100), Color.Blue, _renderer.DefaultShaderProgram));
            _renderer.Flush();
        }

        public static void Update(float fr)
        {
        }
    }
}