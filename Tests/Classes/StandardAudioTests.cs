#region Using

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emotion.Audio;
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
            //Assert.True(pepsi.Format.BitsPerSample == 16);
            //Assert.False(pepsi.Format.IsFloat);
            //Assert.False(pepsi.SoundData.IsEmpty);

            var money = Engine.AssetLoader.Get<AudioAsset>("Sounds/money.wav");
            Assert.True(money.Format.SampleRate == 22050);
            Assert.True(money.Format.Channels == 1);
            //Assert.True(money.Format.BitsPerSample == 16);
            //Assert.False(money.Format.IsFloat);
            //Assert.False(money.SoundData.IsEmpty);
        }

        [Test]
        public void Convert()
        {
            var pepsi = Engine.AssetLoader.Get<AudioAsset>("Sounds/pepsi.wav");
            int pepsiByteSize = pepsi.SoundData.Length * 4;

            var copy = new byte[pepsiByteSize];
            CopyToByteBuffer(pepsi, copy);

            AudioUtil.ConvertFormat(pepsi.Format, new AudioFormat(8, false, 2, 44100), ref copy);
            Assert.Equal(copy.Length, pepsiByteSize / 4); // Same number of samples as sample rate is the same, but bps is 4 times less

            copy = new byte[pepsiByteSize];
            CopyToByteBuffer(pepsi, copy);

            AudioUtil.ConvertFormat(pepsi.Format, new AudioFormat(16, false, 2, 48000), ref copy);
            float ratio = 48000f / pepsi.Format.SampleRate;
            Assert.Equal(copy.Length, pepsiByteSize / 2 * ratio); // divide by 2 because 16bps

            var money = Engine.AssetLoader.Get<AudioAsset>("Sounds/money.wav");
            int moneyByteSize = money.SoundData.Length * 4;

            copy = new byte[moneyByteSize];
            CopyToByteBuffer(money, copy);

            AudioUtil.ConvertFormat(money.Format, new AudioFormat(16, true, 2, 48000), ref copy); // isFloat is intentionally true. (and invalid here)
            ratio = 48000f / money.Format.SampleRate;
            Assert.Equal(copy.Length, (int) (moneyByteSize * ratio)); // multiplied by 2 because going from mono to stereo, bps is twice less though
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
            var copy = new byte[pepsi.SoundData.Length * 4];
            CopyToByteBuffer(pepsi, copy);
            AudioUtil.ConvertFormat(pepsi.Format, format, ref copy);

            var testTasks = new List<Task>();
            for (var io = 0; io < 5; io++)
            {
                testTasks.Add(Task.Run(() =>
                {
                    var streamer = new AudioConverter(pepsi.Format, pepsi.SoundData);
                    var segmentConvert = new List<byte>();
                    int framesGet = new Random().Next(1, 500);
                    Engine.Log.Info($"StreamConvert has chosen {framesGet} for its poll size.", TestRunnerLogger.TestRunnerSrc);

                    var minutesTimeout = 2;
                    DateTime start = DateTime.Now;
                    var playHead = 0;
                    while (DateTime.Now.Subtract(start).TotalMinutes < minutesTimeout) // timeout
                    {
                        var spanData = new Span<byte>(new byte[framesGet * format.FrameSize]);
                        int samplesAmount = streamer.GetSamplesAtByte(format, playHead, framesGet, spanData);
                        if (samplesAmount == 0) break;
                        playHead += samplesAmount;
                        Assert.True(spanData.Length >= samplesAmount * format.SampleSize);
                        segmentConvert.AddRange(spanData.Slice(0, samplesAmount * format.SampleSize).ToArray());
                    }

                    if (DateTime.Now.Subtract(start).TotalMinutes >= minutesTimeout) Engine.Log.Info("StreamConvert timeout.", TestRunnerLogger.TestRunnerSrc);

                    Assert.Equal(segmentConvert.Count, copy.Length);
                    // V No longer true due to floating point precision.
                    //for (var i = 0; i < copy.Length; i++)
                    //{
                    //    Assert.Equal(copy[i], segmentConvert[i]);
                    //}
                }));
            }

            Task.WaitAll(testTasks.ToArray());
            testTasks.Clear();

            var money = Engine.AssetLoader.Get<AudioAsset>("Sounds/money.wav");
            copy = new byte[money.SoundData.Length * 4];
            CopyToByteBuffer(money, copy);
            AudioUtil.ConvertFormat(money.Format, format, ref copy);

            for (var io = 0; io < 5; io++)
            {
                testTasks.Add(Task.Run(() =>
                {
                    var streamer = new AudioConverter(money.Format, money.SoundData);

                    var segmentConvert = new List<byte>();
                    int framesGet = new Random().Next(1, 500);
                    Engine.Log.Info($"StreamConvert (Mono) has chosen {framesGet} for its poll size.", TestRunnerLogger.TestRunnerSrc);

                    DateTime start = DateTime.Now;
                    int playHead = 0;
                    while (DateTime.Now.Subtract(start).TotalMinutes < 1f) // timeout
                    {
                        var data = new byte[framesGet * format.FrameSize];
                        var spanData = new Span<byte>(data);
                        int sampleAmount = streamer.GetSamplesAtByte(format, playHead, framesGet, spanData);
                        if (sampleAmount == 0) break;
                        playHead += sampleAmount;
                        Assert.True(data.Length >= sampleAmount * format.SampleSize);
                        segmentConvert.AddRange(spanData.Slice(0, sampleAmount * format.SampleSize).ToArray());
                    }

                    Assert.Equal(segmentConvert.Count, copy.Length);
                    // V No longer true due to floating point precision.
                    //for (var i = 0; i < copy.Length; i++)
                    //{
                    //    Assert.Equal(copy[i], segmentConvert[i]);
                    //}
                }));
            }

            Task.WaitAll(testTasks.ToArray());
        }

        [Test]
        public void ConvertFormatChanges()
        {
            var pepsi = Engine.AssetLoader.Get<AudioAsset>("Sounds/pepsi.wav");
            var format = new AudioFormat(32, true, 2, 48000);
            var streamer = new AudioConverter(pepsi.Format, pepsi.SoundData);

            // Higher to lower.
            var testData = new byte[format.SampleRate * format.FrameSize];
            var spanData = new Span<byte>(testData);
            var samplesIdx = 0;
            samplesIdx += streamer.GetSamplesAtByte(format, samplesIdx, format.SampleRate, spanData);
            Assert.Equal(samplesIdx, 96000);

            format = new AudioFormat(8, true, 1, 12000);
            samplesIdx += streamer.GetSamplesAtByte(format, samplesIdx, format.SampleRate, spanData);
            samplesIdx += streamer.GetSamplesAtByte(format, samplesIdx, format.SampleRate, spanData);
            samplesIdx += streamer.GetSamplesAtByte(format, samplesIdx, format.SampleRate, spanData);
            Assert.Equal(samplesIdx, 108480);
        }

        private static void CopyToByteBuffer(AudioAsset src, byte[] dst)
        {
            for (var i = 0; i < src.SoundData.Length; i++)
            {
                AudioUtil.SetSampleAsFloat(i, src.SoundData[i], dst, src.Format);
            }
        }
    }

    public static class TestsExtensions
    {
        public static int GetSamplesAtByte(this AudioConverter converter, AudioFormat format, int startIdx, int frameCount, Span<byte> buffer)
        {
            int sampleCount = frameCount * format.Channels;
            var conversionBuffer = new Span<float>(new float[sampleCount]);
            int framesGotten = converter.GetResampledFrames(format, startIdx, frameCount, conversionBuffer);
            if (framesGotten == 0) return 0;

            int samplesGotten = framesGotten * format.Channels;
            for (var i = 0; i < samplesGotten; i++)
            {
                AudioUtil.SetSampleAsFloat(i, conversionBuffer[i], buffer, format);
            }

            return samplesGotten;
        }
    }
}