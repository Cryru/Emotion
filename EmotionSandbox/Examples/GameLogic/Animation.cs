// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine;
using Emotion.Game.Animation;
using Emotion.Game.Layering;
using Emotion.Graphics;
using Emotion.Graphics.GLES;
using Emotion.IO;
using Emotion.Primitives;
using EmotionSandbox.Examples.Generic;
using Soul;

#endregion

namespace EmotionSandbox.Examples.GameLogic
{
    public class Animation : Layer
    {
        private AnimatedTexture _animatedTexture;

        public static void Main()
        {
            // Setup the context and load the loading screen plus this scene in.
            Context.Setup();
            Context.LayerManager.Add(new LoadingScreen(), "__loading__", 0); // The loading screen is below this layer so it is hidden when this layer is loaded.
            Context.LayerManager.Add(new Animation(), "Emotion Game Logic Examples - Animation", 1);
            // Start the context.
            Context.Run();
        }

        public override void Load()
        {
            _animatedTexture = new AnimatedTexture(Context.AssetLoader.Get<Texture>("EmotionLogo.png"), new Vector2(128, 128), AnimationLoopType.Normal, 500, 0, 1);
        }

        public override void Update(float fr)
        {
            _animatedTexture.Update(fr);
        }

        public override void Draw(Renderer renderer)
        {
            // Render a cornflower background to hide the loading screen beneath this layer.
            renderer.Render(new Vector3(0, 0, 0), Context.Settings.RenderSize, Color.CornflowerBlue);

            // Draw a random one of the three textures, and make it transparent red.
            renderer.Render(new Vector3(10, 220, 0), new Vector2(100, 100), Color.White, _animatedTexture.Texture, _animatedTexture.CurrentFrame);
        }

        public override void Unload()
        {
            _animatedTexture = null;
        }
    }
}