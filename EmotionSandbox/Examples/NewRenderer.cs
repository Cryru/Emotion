// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Emotion;
using Emotion.Engine;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;

#endregion

namespace EmotionSandbox.Examples
{
    public class NewRenderer
    {
        private static Renderer _renderer;
        private static Renderable2D _ren;
        private static List<Renderable2D> _rens;

        public static void Main()
        {
            Settings a = new Settings();
            _renderer = new Renderer(a)
            {
                update = Update,
                draw = Draw
            };
            _ren = new Renderable2D(new Vector3(0, 0, 0), new Vector2(100, 100), Color.Blue, Soul.Convert.DegreesToRadians(0));
            _ren.TextureArea = new Rectangle(0, 0, 1, 1);
            //_rens = new List<Renderable2D>();
            //float xehe = 0;
            //float yehe = 0;
            //for (int i = 1; i < 5998; i++)
            //{
            //    _rens.Add(new Renderable2D(new Vector3(xehe, yehe, 0), new Vector2(5, 5), new Color(Soul.Utilities.GenerateRandomNumber(0, 255), Soul.Utilities.GenerateRandomNumber(0, 255), Soul.Utilities.GenerateRandomNumber(0, 255))));
            //    xehe += 5;
            //    if (xehe >= 960)
            //    {
            //        xehe = 0;
            //        yehe += 5;
            //    }
            //}

            Starter.WindowsSetup();
            _loader = new AssetLoader(null);
            _texture = _loader.Get<Emotion.Graphics.GLES.Texture>("test.png");

            _renderer.Run();
        }

        private static AssetLoader _loader;
        private static Emotion.Graphics.GLES.Texture _texture;
        private static float deg = 0;
        private static float x = 0;

        public static void Draw(float fr)
        {
            _texture.Bind();
            _renderer.DefaultShaderProgram.SetUniformInt("drawTexture", 0);

            //_ren.Rotation = Soul.Convert.DegreesToRadians((int) deg);
            //_ren.X = x;
            deg += 1;
            x += 2;
            if (x > 960) x = 0;

            _renderer.Render(new Renderable2D(new Vector3(0, 0, 0), new Vector2(960, 540), Color.Red));
           // _rens.ForEach(_renderer.Render);
            _renderer.Render(_ren);
            _renderer.Flush();
            Console.WriteLine(1000 / fr);
        }

        public static void Update(float fr)
        {
        }
    }
}