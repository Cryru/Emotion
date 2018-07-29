// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
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
            _ren = new Renderable2D(new Vector3(0, 0, 0), new Vector2(100, 100), Color.Blue, Soul.Convert.DegreesToRadians(0));
            _renderer.Run();
        }

        private static float deg = 0;

        public static void Draw(float fr)
        {
            _ren.Rotation = Soul.Convert.DegreesToRadians((int) deg);
            deg += 1;
            _renderer.Render(new Renderable2D(new Vector3(0, 0, 0), new Vector2(960, 540), Color.Red));
            _renderer.Render(_ren);
            _renderer.Flush();
            Console.WriteLine(1000 / fr);
        }

        public static void Update(float fr)
        {
        }
    }
}