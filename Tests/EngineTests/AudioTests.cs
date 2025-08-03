#nullable enable

#region Using

using System;
using Emotion.Core;
using Emotion.Core.Platform;
using Emotion.Core.Systems.Audio;
using Emotion.Core.Systems.IO;
using Emotion.Testing;

#endregion

namespace Tests.EngineTests;

[Test]
public class AudioTests
{
    public class TestAudioContext : AudioContext
    {
        public class TestAudioLayer : AudioLayer
        {
            public PlaybackStatus PreviousStatus { get; protected set; } = PlaybackStatus.NotPlaying;

            public TestAudioLayer(string name) : base(name)
            {
                OnStatusChanged += StatusChanged;
            }

            protected override void UpdateBackend()
            {
            }

            private void StatusChanged(PlaybackStatus oldStatus, PlaybackStatus newStatus)
            {
                PreviousStatus = oldStatus;
            }
        }

        public TestAudioContext(PlatformBase platform) : base(platform)
        {
        }

        public override void AudioLayerProc()
        {
            // nop
        }

        public override AudioLayer CreatePlatformAudioLayer(string layerName)
        {
            return new TestAudioLayer(layerName);
        }
    }

    [Test]
    public void AudioState()
    {
        var ctx = new TestAudioContext(null);
        AudioLayer layer = ctx.CreateLayer("test");

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

        ctx.RemoveLayer("test");
    }

    [Test]
    public void PlaylistLogic()
    {
        var ctx = new TestAudioContext(null);
        AudioLayer layer = ctx.CreateLayer("test");

        var pepsi = Engine.AssetLoader.Get<AudioAsset>("Sounds/pepsi.wav");
        var money = Engine.AssetLoader.Get<AudioAsset>("Sounds/money.wav");

        layer.AddToQueue(pepsi);
        Assert.True(layer.Status == PlaybackStatus.Playing);
        layer.AddToQueue(pepsi);
        layer.PlayNext(money);

        AudioAsset[] playlist = layer.Playlist;
        Assert.True(playlist[0] == pepsi);
        Assert.True(playlist[1] == money);
        Assert.True(playlist[2] == pepsi);
        Assert.True(layer.CurrentTrack.File == pepsi);

        // Advance time ahead.
        ((TestAudioContext.TestAudioLayer)layer).Update((int)(MathF.Ceiling(pepsi.Duration) + 1) * 1000, true);
        Assert.True(layer.CurrentTrack.File == money);

        ctx.RemoveLayer("test");
    }
}