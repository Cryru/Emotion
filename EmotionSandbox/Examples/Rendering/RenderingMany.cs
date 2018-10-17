// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Game.Layering;
using Emotion.Graphics;
using Emotion.Graphics.Batching;
using Emotion.Primitives;
using Emotion.System;
using EmotionSandbox.Examples.Generic;
using Soul;

#endregion

namespace EmotionSandbox.Examples.Rendering
{
    public class RenderingMany : Layer
    {
        public static void Main()
        {
            // Setup the context and load the loading screen plus this scene in.
            Context.Setup();
            Context.LayerManager.Add(new LoadingScreen(), "__loading__", 0); // The loading screen is below this layer so it is hidden when this layer is loaded.
            Context.LayerManager.Add(new RenderingMany(), "Emotion Rendering - Many", 1);
            // Start the context.
            Context.Run();
        }

        private QuadMapBuffer _buffer;

        public override void Load()
        {
            // These operations need to be performed on the GL thread.
            // This can be further optimized by creating the objects on another thread and then just passing them.
            ThreadManager.ExecuteGLThread(() =>
            {
                // Create a new map buffer.
                _buffer = new QuadMapBuffer(Renderer.MaxRenderable);

                // Map buffer.
                int x = 0;
                int y = 0;
                const int size = 5;
                for (int i = 0; i < Renderer.MaxRenderable; i++)
                {
                    // Add objects one after another of a random color.
                    Color randomColor = new Color(Utilities.GenerateRandomNumber(0, 255), Utilities.GenerateRandomNumber(0, 255), Utilities.GenerateRandomNumber(0, 255));
                    _buffer.MapNextQuad(new Vector3(x * size, y * size, 1), new Vector2(size, size), randomColor);

                    x++;
                    if (x * size < 960)
                        continue;
                    x = 0;
                    y++;
                }
            });
        }

        public override void Update(float fr)
        {
        }

        public override void Draw(Renderer renderer)
        {
            // Render a cornflower background to hide the loading screen beneath this layer.
            renderer.Render(new Vector3(0, 0, 0), new Vector2(Context.Host.RenderSize.X, Context.Host.RenderSize.Y), Color.CornflowerBlue);

            // Draw the buffer. This draws everything mapped into it using one draw call, making drawing such objects incredibly efficient.
            renderer.Render(_buffer);

            // You can also pass a model matrix to modify the buffer, applying the change to all vertices mapped into it at once.
            renderer.Render(_buffer, Matrix4.CreateScale(0.25f, 0.25f, 1) * Matrix4.CreateTranslation(400, 400, 1));
        }

        public override void Unload()
        {
        }
    }
}