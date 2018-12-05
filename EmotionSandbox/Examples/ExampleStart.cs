// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Numerics;
using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.Game.UI;
using Emotion.Game.UI.Layout;
using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.Primitives;
using EmotionSandbox.Examples.GameLogic;
using EmotionSandbox.Examples.Generic;
using EmotionSandbox.Examples.Rendering;
using EmotionSandbox.SoundPlayer;

#endregion

namespace EmotionSandbox.Examples
{
    public class ExampleStart : Layer
    {
        #region Assets

        private readonly string _defaultFont = "ExampleFont.ttf";

        #endregion

        private Controller _uiController;
        private Type[] _scenes = {typeof(RenderingMany), typeof(RenderingRectangles), typeof(RenderingText), typeof(RenderingTextures), typeof(SoundPlayerScene), typeof(Animation)};

        public static void Main()
        {
            // Setup the context and load the loading screen plus this scene in.
            Context.Setup();
            Context.LayerManager.Add(new LoadingScreen(), "__loading__", 0); // The loading screen is below this layer so it is hidden when this layer is loaded.
            Context.LayerManager.Add(new ExampleStart(), "example picker", 1); // The loading screen is below this layer so it is hidden when this layer is loaded.
            // Start the context.
            Context.Run();
        }

        public override void Load()
        {
            _uiController = new Controller();
            CornerAnchor cornerAnchor = new CornerAnchor();
            _uiController.Add(cornerAnchor);

            // Create labels for each scene.
            for (int i = 0; i < _scenes.Length; i++)
            {
                int iCopy = i;

                ClickableLabel addSceneLabel = new ClickableLabel(Context.AssetLoader.Get<Font>(_defaultFont), 15, _scenes[i].ToString(), Color.White, Vector3.Zero)
                {
                    OnClick = () =>
                    {
                        Context.LayerManager.Remove(this);
                        Context.LayerManager.Add((Layer) Activator.CreateInstance(_scenes[iCopy]), _scenes[iCopy].ToString(), 1);
                    }
                };
                cornerAnchor.AddChild(addSceneLabel, AnchorLocation.TopLeft, new Rectangle(0, 0, Context.Settings.RenderSettings.Width, 0));
            }
        }

        public override void Update(float frameTime)
        {
            _uiController.Update();
        }

        public override void Draw(Renderer renderer)
        {
            _uiController.Draw();
        }

        public override void Unload()
        {
            _uiController.Dispose();
        }
    }
}