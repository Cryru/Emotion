#region Using

using System;
using System.Collections.Generic;
using System.IO;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Audio;
using Emotion.Test.Helpers;

#endregion

namespace Emotion.Test.Tests
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
            Assert.True(pepsi.SoundData != null);
        }

        [Test]
        public void Convert()
        {
            var pepsi = Engine.AssetLoader.Get<AudioAsset>("Sounds/pepsi.wav");

            var copy = new byte[pepsi.SoundData.Length];
            Array.Copy(pepsi.SoundData, 0, copy, 0, pepsi.SoundData.Length);
            AudioUtil.ConvertFormat(pepsi.Format, new AudioFormat(32, true, 2, 44100), ref copy);
            Assert.True(copy.Length == pepsi.SoundData.Length * 2);

            copy = new byte[pepsi.SoundData.Length];
            Array.Copy(pepsi.SoundData, 0, copy, 0, pepsi.SoundData.Length);
            AudioUtil.ConvertFormat(pepsi.Format, new AudioFormat(16, true, 2, 48000), ref copy); // isFloat is intentionally true.
            float ratio = 48000f / 44100; // Each channel must have been resampled.
            Assert.True(copy.Length == (int) (pepsi.SoundData.Length * ratio));
        }

        [Test]
        public void StreamConvert()
        {
            var pepsi = Engine.AssetLoader.Get<AudioAsset>("Sounds/pepsi.wav");

            var format = new AudioFormat(32, true, 2, 48000);
            var copy = new byte[pepsi.SoundData.Length];
            Array.Copy(pepsi.SoundData, 0, copy, 0, pepsi.SoundData.Length);
            AudioUtil.ConvertFormat(pepsi.Format, format, ref copy);

            var streamer = new AudioStreamer(pepsi.Format, pepsi.SoundData);
            streamer.SetConvertFormat(format);

            for (var io = 0; io < 5; io++)
            {
                var segmentConvert = new List<byte>();
                int framesGet = new Random().Next(1, 500);
                Engine.Log.Info($"StreamConvert has chosen {framesGet} for its poll size.", CustomMSource.TestRunner);

                DateTime start = DateTime.Now;
                while (DateTime.Now.Subtract(start).TotalMinutes < 1f) // timeout
                {
                    int frameAmount = streamer.GetNextFrames(framesGet, out byte[] data);
                    if (frameAmount == 0) break;
                    Assert.Equal(data.Length, frameAmount * format.SampleSize);
                    segmentConvert.AddRange(data);
                }

                Assert.Equal(segmentConvert.Count, copy.Length);
                for (var i = 0; i < copy.Length; i++)
                {
                    Assert.Equal(copy[i], segmentConvert[i]);
                }

                streamer.Reset();
            }
        }
    }
}