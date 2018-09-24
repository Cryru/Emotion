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
    public class CenterAnchorExample : Layer
    {
        private Controller _uiController;
        private CenterAnchor _anchor;

        public static void Main()
        {
            // Get the context and load the loading screen plus this scene in.
            Context context = Starter.GetEmotionContext();
            context.LayerManager.Add(new LoadingScreen(), "__loading__", 0); // The loading screen is below this layer so it is hidden when this layer is loaded.
            context.LayerManager.Add(new CenterAnchorExample(), "Emotion Systems - UI", 1);
            // Start the context.
            context.Start();
        }

        public override void Load()
        {
            // Create the corner anchor.
            _anchor = new CenterAnchor();

            // Create the UI controller.
            _uiController = new Controller(Context);

            // Create a component to center.
            BasicButton test = new BasicButton(new Vector3(10, 10, 0), new Vector2( 100, 100));
            _uiController.Add(test);
            _anchor.AddControl(test, new Rectangle(0, 100, 0, 0));

            _uiController.Add(_anchor);
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