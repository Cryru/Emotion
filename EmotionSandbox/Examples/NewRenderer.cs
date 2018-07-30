// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using Emotion;
using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.Graphics;
using Emotion.Graphics.GLES;
using Emotion.Primitives;
using Soul;
using Convert = Soul.Convert;

#endregion

namespace EmotionSandbox.Examples
{
    public class NewRenderer : Layer
    {
        private static Renderable2D _ren;
        private static List<Renderable2D> _renderables;
        private float _rotationDegree = 0;

        public static void Main()
        {
            Context context = Starter.GetEmotionContext();
            context.LayerManager.Add(new LoadingScreen(), "__loading__", -1);
            context.LayerManager.Add(new NewRenderer(), "Emotion Rendering - Many Objects", 1);
            context.Start();
        }

        public override void Load()
        {
            _ren = new Renderable2D(new Vector3(0, 0, 0), new Vector2(100, 100), Color.Blue, Convert.DegreesToRadians(0))
            {
                Texture = Context.AssetLoader.Get<Texture>("1.png")
            };

            _renderables = new List<Renderable2D>();
            float x = 0;
            float y = 0;
            float sizeX = 10;
            float sizeY = 10;
            for (int i = 0; i < Renderer.MaxRenderable; i++)
            {
                _renderables.Add(new Renderable2D(new Vector3(x, y, 0), new Vector2(sizeX, sizeY),
                    new Color(Utilities.GenerateRandomNumber(0, 255), Utilities.GenerateRandomNumber(0, 255), Utilities.GenerateRandomNumber(0, 255))));
                x += sizeX;
                if (!(x >= Context.Host.RenderSize.X)) continue;
                x = 0;
                y += sizeY;
            }
        }

        public override void Update(float fr)
        {
        }

        public override void Draw(Renderer renderer)
        {
            //_ren.Rotation = Convert.DegreesToRadians((int) deg);
            //_ren.X = x;
            //deg += 1;
            //x += 2;
            //if (x > 960) x = 0;

            _rotationDegree += 0.01f * Context.FrameTime;
            if (_rotationDegree == 361) _rotationDegree = 1;

            renderer.Render(new Vector3(0, 0, 0), new Vector2(Context.Host.RenderSize.X, Context.Host.RenderSize.Y), Color.Black, null, Rectangle.Empty);

            renderer.RenderFlush();
            Matrix4 rotationMatrix = Matrix4.CreateTranslation(5, 5, 1).Inverted() * Matrix4.CreateRotationZ((int) _rotationDegree) * Matrix4.CreateTranslation(5, 5, 1);
            renderer.TransformationStack.Push(rotationMatrix);
            _renderables.ForEach(renderer.Render);
            renderer.TransformationStack.Pop();
            renderer.RenderFlush();
            renderer.RenderOutline(new Vector3(10, 10, 0), new Vector2(50, 50), Color.Red);

            // _renderer.Render(new Renderable2D(new Vector3(0, 0, 0), new Vector2(960, 540), Color.Red));

            //_renderer.Render(_ren);



            Console.WriteLine(1000 / Context.FrameTime);
        }

        public override void Unload()
        {
        }
    }
}