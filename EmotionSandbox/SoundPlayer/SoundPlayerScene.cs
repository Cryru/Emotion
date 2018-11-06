using System;
using Emotion.Game.Layering;
using Emotion.Graphics;
using Emotion.Engine;
using Emotion.Game.UI;
using Emotion.Game.UI.Layout;
using Emotion.Graphics.GLES;
using Emotion.Graphics.Text;
using Emotion.IO;
using Emotion.Primitives;
using EmotionSandbox.Examples.Generic;

namespace EmotionSandbox.SoundPlayer
{
    public class SoundPlayerScene : Layer
    {
        #region Assets

        private static string _playButton = "SoundPlayer/playButton.png";
        private static string _defaultFont = "SoundPlayer/RobotoFont.woff2";

        #endregion

        private Controller _uiController;

        public static void Main()
        {
            Context.Setup();

            Context.LayerManager.Add(new LoadingScreen(), "__loading__", 0);
            Context.LayerManager.Add(new SoundPlayerScene(), "soundPlayer", 1);

            Context.Run();
        }

        public override void Load()
        {
            
            Context.AssetLoader.Get<Font>(_defaultFont);

            _uiController = new Controller();
            CenterAnchor centerAnchor = new CenterAnchor();
            _uiController.Add(centerAnchor);

            BasicButton playButton = new BasicButton(Vector3.Zero, new Vector2(36, 36)) {Texture = Context.AssetLoader.Get<Texture>(_playButton)};
            centerAnchor.AddChild(playButton, new Rectangle(0, Context.Settings.RenderHeight / 2 - 36, 0, 0));
        }

        public override void Update(float frameTime)
        {
            _uiController.Update();
        }

        public override void Draw(Renderer renderer)
        {
            // Render a cornflower background to hide the loading screen beneath this layer.
            renderer.Render(new Vector3(0, 0, 0), new Vector2(Context.Host.RenderSize.X, Context.Host.RenderSize.Y), Color.CornflowerBlue);

            _uiController.Draw();
        }

        public override void Unload()
        {

        }
    }
}
