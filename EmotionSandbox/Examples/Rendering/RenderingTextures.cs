// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.Graphics;
using Emotion.Graphics.GLES;
using Emotion.Primitives;
using Emotion.Engine.Threading;
using EmotionSandbox.Examples.Generic;
using Soul;

#endregion

namespace EmotionSandbox.Examples.Rendering
{
    public class RenderingTextures : Layer
    {
        private int _random = 1;
        private float _randomTimer;

        public static void Main()
        {
            // Setup the context and load the loading screen plus this scene in.
            Context.Setup();
            Context.LayerManager.Add(new LoadingScreen(), "__loading__", 0); // The loading screen is below this layer so it is hidden when this layer is loaded.
            Context.LayerManager.Add(new RenderingTextures(), "Emotion Rendering - Textures", 1);
            // Start the context.
            Context.Run();
        }

        public override void Load()
        {
            // Load the three textures using the asset loader.
            Context.AssetLoader.Get<Texture>("1.png");
            Context.AssetLoader.Get<Texture>("2.png");
            Context.AssetLoader.Get<Texture>("3.png");
        }

        public override void Update(float fr)
        {
            // Change the random number for the third texture every second.
            _randomTimer += fr;
            if (_randomTimer >= 1000)
            {
                _randomTimer -= 1000;
                _random = Utilities.GenerateRandomNumber(1, 3);
            }
        }

        public override void Draw(Renderer renderer)
        {
            // Render a cornflower background to hide the loading screen beneath this layer.
            renderer.Render(new Vector3(0, 0, 0), new Vector2(Context.Host.RenderSize.X, Context.Host.RenderSize.Y), Color.CornflowerBlue);

            // Render a 100, 100 rectangle at 10, 10 and texture it using the loaded texture.
            // The color is white because we don't want to tint the texture. The texture is not loaded again by the asset loader as it was loaded in the Load function.
            renderer.Render(new Vector3(10, 10, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("1.png"));

            // Render a second rectangle. This time only a part of the texture is drawn.
            renderer.Render(new Vector3(10, 115, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("2.png"), new Rectangle(0, 0, 90, 90));

            // Draw a random one of the three textures, and make it transparent red.
            renderer.Render(new Vector3(10, 220, 0), new Vector2(100, 100), new Color(255, 0, 0, 125), Context.AssetLoader.Get<Texture>(_random + ".png"));
        }

        public override void Unload()
        {
            // Unload the texture with the layer.
            // Be wary of resources used by multiple layers. The cached assets are shared between all of them.
            // If a needed asset is unloaded it will just be loaded again, but this may cause slowdowns.
            Context.AssetLoader.Destroy("1.png");
            Context.AssetLoader.Destroy("2.png");
            Context.AssetLoader.Destroy("3.png");
        }
    }
}