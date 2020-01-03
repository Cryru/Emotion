#region Using

using System;
using Emotion.Audio;
using Emotion.Common;
using Emotion.IO;
using Emotion.Platform.Implementation.Null;
using Emotion.Test;

#endregion

namespace Tests.Classes
{
    [Test("Audio", true)]
    public class AudioTests
    {
        [Test]
        public void AudioState()
        {
            var nullAudio = new NullAudioContext();
            AudioLayer layer = nullAudio.CreateLayer("test");

            var pepsi = Engine.AssetLoader.Get<AudioAsset>("Sounds/pepsi.wav");
            var money = Engine.AssetLoader.Get<AudioAsset>("Sounds/money.wav");

            Assert.True(layer.Status == PlaybackStatus.NotPlaying);
            layer.PlayNext(pepsi);
            Assert.True(layer.Status == PlaybackStatus.Playing);
            layer.Stop();
            Assert.True(layer.Status == PlaybackStatus.NotPlaying);

            layer.Pause();
            Assert.True(layer.Status == PlaybackStatus.Paused);
            layer.PlayNext(money);
            Assert.True(layer.Status == PlaybackStatus.Paused);
            layer.Resume();
            Assert.True(layer.Status == PlaybackStatus.Playing);
            layer.Stop();
            Assert.True(layer.Status == PlaybackStatus.NotPlaying);

            nullAudio.RemoveLayer("test");
        }

        [Test]
        public void PlaylistLogic()
        {
            var nullAudio = new NullAudioContext();
            AudioLayer layer = nullAudio.CreateLayer("test");

            var pepsi = Engine.AssetLoader.Get<AudioAsset>("Sounds/pepsi.wav");
            var money = Engine.AssetLoader.Get<AudioAsset>("Sounds/money.wav");

            layer.AddToQueue(pepsi);
            Assert.True(layer.Status == PlaybackStatus.Playing);
            layer.AddToQueue(pepsi);
            layer.PlayNext(money);

            AudioAsset[] playlist = layer.Playlist;
            Assert.True(playlist[0] == pepsi);
            Assert.True(playlist[1] == money);
            Assert.True(playlist[0] == pepsi);
            Assert.True(layer.CurrentTrack.File == pepsi);

            ((NullAudioLayer) layer).AdvanceTime((int) MathF.Ceiling(pepsi.Duration));
            Assert.True(layer.CurrentTrack.File == money);

            nullAudio.RemoveLayer("test");
        }
    }
}