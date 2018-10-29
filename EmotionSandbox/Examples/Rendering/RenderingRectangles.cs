// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Game.Layering;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Engine;
using EmotionSandbox.Examples.Generic;

#endregion

namespace EmotionSandbox.Examples.Rendering
{
    public class RenderingRectangles : Layer
    {
        public static void Main()
        {
            // Setup the context and load the loading screen plus this scene in.
            Context.Setup();
            Context.LayerManager.Add(new LoadingScreen(), "__loading__", 0); // The loading screen is below this layer so it is hidden when this layer is loaded.
            Context.LayerManager.Add(new RenderingRectangles(), "Emotion Rendering - Rectangles", 1);
            // Start the context.
            Context.Run();
        }

        public override void Load()
        {
        }

        public override void Update(float fr)
        {
        }

        public override void Draw(Renderer renderer)
        {
            // Render a cornflower background to hide the loading screen beneath this layer.
            renderer.Render(new Vector3(0, 0, 0), new Vector2(Context.Host.RenderSize.X, Context.Host.RenderSize.Y), Color.CornflowerBlue);

            // Render a red square at 10, 10 with size 10, 10
            renderer.Render(new Vector3(10, 10, 0), new Vector2(10, 10), Color.Red);

            // Render a rectangle outline below it.
            renderer.RenderOutline(new Vector3(10, 25, 0), new Vector2(10, 10), Color.Red);

            // Render two overlapping rectangles. As their color alpha is not 255 they will be transparent.
            renderer.Render(new Vector3(10, 40, 0), new Vector2(10, 10), new Color(255, 0, 0, 125));
            renderer.Render(new Vector3(15, 45, 0), new Vector2(10, 10), new Color(0, 0, 255, 125));
        }

        public override void Unload()
        {
        }
    }
}