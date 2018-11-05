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

        private Controller _UIController;

        public static void Main()
        {
            Context.Setup();

            Context.LayerManager.Add(new LoadingScreen(), "__loading__", 0);
            Context.LayerManager.Add(new SoundPlayerScene(), "soundPlayer", 1);
        }

        public override void Load()
        {
            
            Context.AssetLoader.Get<Font>(_defaultFont);

            _UIController = new Controller();
            CenterAnchor centerAnchor = new CenterAnchor();
            _UIController.Add(centerAnchor);

            BasicButton playButton = new BasicButton(Vector3.Zero, new Vector2(36, 36)) {Texture = Context.AssetLoader.Get<Texture>(_playButton)};
            centerAnchor.AddChild(playButton);
        }

        public override void Update(float frameTime)
        {

        }

        public override void Draw(Renderer renderer)
        {
            
        }

        public override void Unload()
        {

        }
    }
}
