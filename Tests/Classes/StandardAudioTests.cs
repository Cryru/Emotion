#region Using

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Audio;
using Emotion.Test;
using Emotion.Test.Helpers;

#endregion

namespace Tests.Classes
{
    [Test("StandardAudio", true)]
    public class StandardAudioTests
    {
        [Test]
        public void ReadWav()
        {
            var pepsi = Engine.AssetLoader.Get<AudioAsset>("Sounds/pepsi.wav");
            Assert.True(pepsi.Format.SampleRate == 44100);
            Assert.True(pepsi.Format.Channels == 2);
            Assert.True(pepsi.Format.BitsPerSample == 16);
            Assert.False(pepsi.Format.IsFloat);
            Assert.False(pepsi.SoundData.IsEmpty);

            var money = Engine.AssetLoader.Get<AudioAsset>("Sounds/money.wav");
            Assert.True(money.Format.SampleRate == 22050);
            Assert.True(money.Format.Channels == 1);
            Assert.True(money.Format.BitsPerSample == 16);
            Assert.False(money.Format.IsFloat);
            Assert.False(money.SoundData.IsEmpty);
        }

        [Test]
        public void Convert()
        {
            var pepsi = Engine.AssetLoader.Get<AudioAsset>("Sounds/pepsi.wav");

            var copy = new byte[pepsi.SoundData.Length];
            pepsi.SoundData.CopyTo(copy);
            AudioUtil.ConvertFormat(pepsi.Format, new AudioFormat(32, true, 2, 44100), ref copy);
            Assert.True(copy.Length == pepsi.SoundData.Length * 2);

            copy = new byte[pepsi.SoundData.Length];
            pepsi.SoundData.CopyTo(copy);
            AudioUtil.ConvertFormat(pepsi.Format, new AudioFormat(16, true, 2, 48000), ref copy); // isFloat is intentionally true.
            float ratio = 48000f / pepsi.Format.SampleRate;
            Assert.True(copy.Length == (int) (pepsi.SoundData.Length * ratio));

            var money = Engine.AssetLoader.Get<AudioAsset>("Sounds/money.wav");

            copy = new byte[money.SoundData.Length];
            money.SoundData.CopyTo(copy);
            AudioUtil.ConvertFormat(money.Format, new AudioFormat(16, true, 2, 48000), ref copy); // isFloat is intentionally true.
            ratio = 48000f / money.Format.SampleRate;
            Assert.True(copy.Length == (int) (money.SoundData.Length * 2 * ratio));
        }

        /// <summary>
        /// Verifies that converting a stream in segments produces the same results are converting it as a whole.
        /// This matters because the resampling is different.
        /// </summary>
        [Test]
        public void StreamConvert()
        {
            var pepsi = Engine.AssetLoader.Get<AudioAsset>("Sounds/pepsi.wav");

            var format = new AudioFormat(32, true, 2, 48000);
            var copy = new byte[pepsi.SoundData.Length];
            pepsi.SoundData.CopyTo(copy);
            AudioUtil.ConvertFormat(pepsi.Format, format, ref copy);

            var testTasks = new List<Task>();
            for (var io = 0; io < 5; io++)
            {
                testTasks.Add(Task.Run(() =>
                {
                    var streamer = new AudioStreamer(pepsi.Format, pepsi.SoundData);
                    streamer.SetConvertFormat(format);
                    var segmentConvert = new List<byte>();
                    int framesGet = new Random().Next(1, 500);
                    Engine.Log.Info($"StreamConvert has chosen {framesGet} for its poll size.", CustomMSource.TestRunner);

                    var minutesTimeout = 2;
                    DateTime start = DateTime.Now;
                    while (DateTime.Now.Subtract(start).TotalMinutes < minutesTimeout) // timeout
                    {
                        var data = new byte[framesGet * format.FrameSize];
                        var spanData = new Span<byte>(data);
                        int frameAmount = streamer.GetNextFrames(framesGet, spanData);
                        if (frameAmount == 0) break;
                        Assert.True(data.Length >= frameAmount * format.FrameSize);
                        segmentConvert.AddRange(spanData.Slice(0, frameAmount * format.FrameSize).ToArray());
                    }

                    if (DateTime.Now.Subtract(start).TotalMinutes >= minutesTimeout) Engine.Log.Info("StreamConvert timeout.", CustomMSource.TestRunner);

                    Assert.Equal(segmentConvert.Count, copy.Length);
                    for (var i = 0; i < copy.Length; i++)
                    {
                        Assert.Equal(copy[i], segmentConvert[i]);
                    }
                }));
            }

            Task.WaitAll(testTasks.ToArray());
            testTasks.Clear();

            var money = Engine.AssetLoader.Get<AudioAsset>("Sounds/money.wav");
            copy = new byte[money.SoundData.Length];
            money.SoundData.CopyTo(copy);
            AudioUtil.ConvertFormat(money.Format, format, ref copy);

            for (var io = 0; io < 5; io++)
            {
                testTasks.Add(Task.Run(() =>
                {
                    var streamer = new AudioStreamer(money.Format, money.SoundData);
                    streamer.SetConvertFormat(format);

                    var segmentConvert = new List<byte>();
                    int framesGet = new Random().Next(1, 500);
                    Engine.Log.Info($"StreamConvert (Mono) has chosen {framesGet} for its poll size.", CustomMSource.TestRunner);

                    DateTime start = DateTime.Now;
                    while (DateTime.Now.Subtract(start).TotalMinutes < 1f) // timeout
                    {
                        var data = new byte[framesGet * format.FrameSize];
                        var spanData = new Span<byte>(data);
                        int frameAmount = streamer.GetNextFrames(framesGet, spanData);
                        if (frameAmount == 0) break;
                        Assert.True(data.Length >= frameAmount * format.FrameSize);
                        segmentConvert.AddRange(spanData.Slice(0, frameAmount * format.FrameSize).ToArray());
                    }

                    Assert.Equal(segmentConvert.Count, copy.Length);
                    for (var i = 0; i < copy.Length; i++)
                    {
                        Assert.Equal(copy[i], segmentConvert[i]);
                    }
                }));
            }

            Task.WaitAll(testTasks.ToArray());
        }

        [Test]
        public void ConvertFormatChanges()
        {
            var pepsi = Engine.AssetLoader.Get<AudioAsset>("Sounds/pepsi.wav");
            var format = new AudioFormat(32, true, 2, 48000);
            var streamer = new AudioStreamer(pepsi.Format, pepsi.SoundData);
            streamer.SetConvertFormat(format);

            // Higher to lower.
            var testData = new byte[format.SampleRate * format.FrameSize];
            var spanData = new Span<byte>(testData);
            streamer.GetNextFrames(format.SampleRate, spanData);

            format = new AudioFormat(8, true, 1, 12000);
            streamer.SetConvertFormat(format);
            streamer.GetNextFrames(format.SampleRate, spanData);
            streamer.GetNextFrames(format.SampleRate, spanData);
            streamer.GetNextFrames(format.SampleRate, spanData);
        }
    }
}