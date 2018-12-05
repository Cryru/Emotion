// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Numerics;
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
            // Setup the context and load the loading screen plus this scene in.
            Context.Setup();
            Context.LayerManager.Add(new LoadingScreen(), "__loading__", 0); // The loading screen is below this layer so it is hidden when this layer is loaded.
            Context.LayerManager.Add(new CenterAnchorExample(), "Emotion Systems - UI", 1);
            // Start the context.
            Context.Run();
        }

        public override void Load()
        {
            // Create the UI controller.
            _uiController = new Controller();

            // Create the corner anchor.
            _anchor = new CenterAnchor();
            _uiController.Add(_anchor);

            // Create a component to center.
            BasicButton test = new BasicButton(new Vector3(10, 10, 0), new Vector2(100, 100));
            _anchor.AddChild(test, new Rectangle(0, 100, 0, 0));
        }

        public override void Update(float frameTime)
        {
            // Update the controller.
            _uiController.Update();
        }

        public override void Draw(Renderer renderer)
        {
            // Render a cornflower background to hide the loading screen beneath this layer.
            renderer.Render(new Vector3(0, 0, 0), Context.Settings.RenderSettings.Size, Color.CornflowerBlue);

            // Draw the controller.
            _uiController.Draw();
        }

        public override void Unload()
        {
        }
    }
}