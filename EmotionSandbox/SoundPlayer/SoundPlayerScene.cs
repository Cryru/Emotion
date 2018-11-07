// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.Game.UI;
using Emotion.Game.UI.Layout;
using Emotion.Graphics;
using Emotion.Graphics.GLES;
using Emotion.Graphics.Text;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Sound;
using EmotionSandbox.Examples.Generic;

#endregion

namespace EmotionSandbox.SoundPlayer
{
    public class SoundPlayerScene : Layer
    {
        #region Assets

        private readonly string _playButton = "SoundPlayer/playButton.png";
        private readonly string _pauseButton = "SoundPlayer/pauseButton.png";
        private readonly string _loopButton = "SoundPlayer/loopButton.png";
        private readonly string _addButton = "SoundPlayer/addButton.png";

        private readonly string _defaultFont = "SoundPlayer/RobotoFont.woff";
        private readonly string[] _sounds = {"SoundPlayer/ElectricSleepMainMenu.wav", "SoundPlayer/up.wav", "SoundPlayer/down.wav"};

        #endregion

        private Controller _uiController;
        private BasicButton _controlButton;
        private ScrollInput _soundBar;
        private BasicText _soundBarInfo;
        private bool _append;

        public static void Main()
        {
            Context.Setup();

            Context.LayerManager.Add(new LoadingScreen(), "__loading__", 0);
            Context.LayerManager.Add(new SoundPlayerScene(), "soundPlayer", 1);

            Context.Run();
        }

        public override void Load()
        {
            _uiController = new Controller();
            CenterAnchor centerAnchor = new CenterAnchor();
            _uiController.Add(centerAnchor);

            _controlButton = new BasicButton(Vector3.Zero, new Vector2(36, 36))
            {
                Texture = Context.AssetLoader.Get<Texture>(_playButton),
                OnClick = () =>
                {
                    SoundLayer layer = Context.SoundManager.GetLayer("main");
                    if (layer == null) return;

                    if (layer.Status == SoundStatus.Playing)
                        layer.Pause();
                    else
                        layer.Resume();
                }
            };
            centerAnchor.AddChild(_controlButton, new Rectangle(0, Context.Settings.RenderHeight / 2 - 36, 0, 0));

            BasicButton loopButton = new BasicButton(Vector3.Zero, new Vector2(36, 36))
            {
                Texture = Context.AssetLoader.Get<Texture>(_loopButton)
            };
            loopButton.OnClick = () =>
            {
                SoundLayer layer = Context.SoundManager.GetLayer("main");
                if (layer == null) return;

                layer.Looping = !layer.Looping;
                loopButton.Tint = layer.Looping ? Color.Green : Color.White;
            };
            centerAnchor.AddChild(loopButton, new Rectangle(-40, Context.Settings.RenderHeight / 2 - 36, 0, 0));

            _soundBar = new ScrollInput(Vector3.Zero, new Vector2(Context.Host.RenderSize.X - 40, 10))
            {
                KeepSelectorInside = true,
                SelectorRatio = 3
            };
            centerAnchor.AddChild(_soundBar, new Rectangle(0, Context.Settings.RenderHeight / 2 - 10, 0, 0));

            _soundBarInfo = new BasicText(Context.AssetLoader.Get<Font>(_defaultFont), 13, "N/A", Color.White, Vector3.Zero);
            centerAnchor.AddChild(_soundBarInfo, new Rectangle(100, Context.Settings.RenderHeight / 2 - 30, 0, 0));

            CornerAnchor cornerAnchor = new CornerAnchor();
            _uiController.Add(cornerAnchor);

            for (int i = 0; i < _sounds.Length; i++)
            {
                int iCopy = i;

                ClickableLabel addSoundText = new ClickableLabel(Context.AssetLoader.Get<Font>(_defaultFont), 15, _sounds[i], Color.White, Vector3.Zero)
                {
                    OnClick = () =>
                    {
                        if (_append)
                            Context.SoundManager.PlayQueue(Context.AssetLoader.Get<SoundFile>(_sounds[iCopy]), "main");
                        else
                            Context.SoundManager.Play(Context.AssetLoader.Get<SoundFile>(_sounds[iCopy]), "main");
                    }
                };
                cornerAnchor.AddChild(addSoundText, AnchorLocation.TopLeft, new Rectangle(0, 0, Context.Host.RenderSize.X, 0));
            }

            BasicButton appendButton = new BasicButton(Vector3.Zero, new Vector2(36, 36))
            {
                Texture = Context.AssetLoader.Get<Texture>(_addButton)
            };
            appendButton.OnClick = () =>
            {
                _append = !_append;
                appendButton.Tint = _append ? Color.Green : Color.White;
            };
            cornerAnchor.AddChild(appendButton, AnchorLocation.TopLeft, new Rectangle(0, 0, Context.Host.RenderSize.X, 0));

            PlayerVolumeControl volumeControl = new PlayerVolumeControl(Vector3.Zero, new Vector2(100, 20));
            cornerAnchor.AddChild(volumeControl, AnchorLocation.TopRight, new Rectangle(0, 20, 20, 0));
        }

        public override void Update(float frameTime)
        {
            _uiController.Update();

            SoundLayer mainLayer = Context.SoundManager.GetLayer("main");
            if (mainLayer == null) return;

            _controlButton.Texture = Context.AssetLoader.Get<Texture>(Context.SoundManager.GetLayer("main")?.Status == SoundStatus.Playing ? _pauseButton : _playButton);

            if (mainLayer.CurrentlyPlayingFile == null)
            {
                _soundBar.Value = 0;
                _soundBarInfo.Text = "N/A";
            }
            else
            {
                int playbackPercentage = (int) (100 * mainLayer.PlaybackLocation / mainLayer.TotalDuration);
                _soundBar.Value = playbackPercentage;
                _soundBarInfo.Text = $"{mainLayer.PlaybackLocation}:{mainLayer.TotalDuration}";
            }
        }

        public override void Draw(Renderer renderer)
        {
            // Render a cornflower background to hide the loading screen beneath this layer.
            renderer.Render(new Vector3(0, 0, 0), new Vector2(Context.Host.RenderSize.X, Context.Host.RenderSize.Y), Color.CornflowerBlue);

            _uiController.Draw();
        }

        public override void Unload()
        {
            _uiController.Dispose();
        }
    }
}