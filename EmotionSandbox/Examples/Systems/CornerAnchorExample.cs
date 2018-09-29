// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion;
using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.Game.UI;
using Emotion.Game.UI.Layout;
using Emotion.Graphics;
using Emotion.Primitives;
using EmotionSandbox.Examples.Generic;

#endregion

namespace EmotionSandbox.Examples.Systems
{
    public class CornerAnchorExample : Layer
    {
        private Controller _uiController;
        private CornerAnchor _anchor;

        public static void Main()
        {
            // Get the context and load the loading screen plus this scene in.
            Context context = Starter.GetEmotionContext();
            context.LayerManager.Add(new LoadingScreen(), "__loading__", 0); // The loading screen is below this layer so it is hidden when this layer is loaded.
            context.LayerManager.Add(new CornerAnchorExample(), "Emotion Systems - UI", 1);
            // Start the context.
            context.Start();
        }

        public override void Load()
        {
            // Create the corner anchor.
            _anchor = new CornerAnchor {Padding = new Rectangle(10, 10, 10, 10)};

            // Create the UI controller.
            _uiController = new Controller(Context);
            _uiController.Add(_anchor);

            // Generate 10 random components in each corner.
            Random randomizer = new Random();
            for (int i = 0; i < 10; i++)
            {
                BasicButton test = new BasicButton(new Vector3(0, 0, 0), new Vector2(randomizer.Next(10, 101), randomizer.Next(10, 101)));
                _anchor.AddControl(test, AnchorLocation.TopLeft, new Rectangle(randomizer.Next(5, 16), randomizer.Next(5, 16), randomizer.Next(5, 16), randomizer.Next(5, 16)));
            }

            for (int i = 0; i < 10; i++)
            {
                BasicButton test = new BasicButton(new Vector3(0, 0, 0), new Vector2(randomizer.Next(10, 101), randomizer.Next(10, 101)));
                _anchor.AddControl(test, AnchorLocation.BottomLeft, new Rectangle(randomizer.Next(5, 16), randomizer.Next(5, 16), randomizer.Next(5, 16), randomizer.Next(5, 16)));
            }

            for (int i = 0; i < 10; i++)
            {
                BasicButton test = new BasicButton(new Vector3(0, 0, 0), new Vector2(randomizer.Next(10, 101), randomizer.Next(10, 101)));
                _anchor.AddControl(test, AnchorLocation.TopRight, new Rectangle(randomizer.Next(5, 16), randomizer.Next(5, 16), randomizer.Next(5, 16), randomizer.Next(5, 16)));
            }

            for (int i = 0; i < 10; i++)
            {
                BasicButton test = new BasicButton(new Vector3(0, 0, 0), new Vector2(randomizer.Next(10, 101), randomizer.Next(10, 101)));
                _anchor.AddControl(test, AnchorLocation.BottomRight, new Rectangle(randomizer.Next(5, 16), randomizer.Next(5, 16), randomizer.Next(5, 16), randomizer.Next(5, 16)));
            }
        }

        public override void Update(float frameTime)
        {
            // Update the controller.
            _uiController.Update();
        }

        public override void Draw(Renderer renderer)
        {
            // Render a cornflower background to hide the loading screen beneath this layer.
            renderer.Render(new Vector3(0, 0, 0), new Vector2(Context.Host.RenderSize.X, Context.Host.RenderSize.Y), Color.CornflowerBlue);

            // Draw the controller.
            _uiController.Draw();
        }

        public override void Unload()
        {
        }
    }
}