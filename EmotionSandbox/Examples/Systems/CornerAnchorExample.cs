﻿// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
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
    public class CornerAnchorExample : Layer
    {
        private Controller _uiController;
        private CornerAnchor _anchor;

        public static void Main()
        {
            // Setup the context and load the loading screen plus this scene in.
            Context.Setup();
            Context.LayerManager.Add(new LoadingScreen(), "__loading__", 0); // The loading screen is below this layer so it is hidden when this layer is loaded.
            Context.LayerManager.Add(new CornerAnchorExample(), "Emotion Systems - UI", 1);
            // Start the context.
            Context.Run();
        }

        public override void Load()
        {
            // Create the corner anchor.
            _anchor = new CornerAnchor {Padding = new Rectangle(10, 10, 10, 10)};

            // Create the UI controller.
            _uiController = new Controller();
            _uiController.Add(_anchor);

            // Generate 10 random components in each corner.
            Random randomizer = new Random();
            for (int i = 0; i < 10; i++)
            {
                BasicButton test = new BasicButton(new Vector3(0, 0, 0), new Vector2(randomizer.Next(10, 101), randomizer.Next(10, 101)));
                _anchor.AddChild(test, AnchorLocation.TopLeft, new Rectangle(randomizer.Next(5, 16), randomizer.Next(5, 16), randomizer.Next(5, 16), randomizer.Next(5, 16)));
            }

            for (int i = 0; i < 10; i++)
            {
                BasicButton test = new BasicButton(new Vector3(0, 0, 0), new Vector2(randomizer.Next(10, 101), randomizer.Next(10, 101)));
                _anchor.AddChild(test, AnchorLocation.BottomLeft, new Rectangle(randomizer.Next(5, 16), randomizer.Next(5, 16), randomizer.Next(5, 16), randomizer.Next(5, 16)));
            }

            for (int i = 0; i < 10; i++)
            {
                BasicButton test = new BasicButton(new Vector3(0, 0, 0), new Vector2(randomizer.Next(10, 101), randomizer.Next(10, 101)));
                _anchor.AddChild(test, AnchorLocation.TopRight, new Rectangle(randomizer.Next(5, 16), randomizer.Next(5, 16), randomizer.Next(5, 16), randomizer.Next(5, 16)));
            }

            for (int i = 0; i < 10; i++)
            {
                BasicButton test = new BasicButton(new Vector3(0, 0, 0), new Vector2(randomizer.Next(10, 101), randomizer.Next(10, 101)));
                _anchor.AddChild(test, AnchorLocation.BottomRight, new Rectangle(randomizer.Next(5, 16), randomizer.Next(5, 16), randomizer.Next(5, 16), randomizer.Next(5, 16)));
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
            renderer.Render(new Vector3(0, 0, 0), Context.Settings.RenderSettings.Size, Color.CornflowerBlue);

            // Draw the controller.
            _uiController.Draw();
        }

        public override void Unload()
        {
        }
    }
}