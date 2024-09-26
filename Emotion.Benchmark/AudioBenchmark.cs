#region Using

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using Emotion.Audio;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Audio;
using Emotion.Standard.Audio.WAV;
using Emotion.Standard.Image.ImgBin;
using Emotion.Standard.Image.PNG;
using Emotion.Standard.Logging;
using Emotion.Testing;
using Emotion.Utility;
using System.Numerics;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Benchmark;

[MemoryDiagnoser]
public class AudioBenchmark
{
    private AudioAsset _audioAsset = null!;
    private AudioFormat _targetFormat = new AudioFormat(16, false, 2, 48_000);
    private float[] _output = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        Configurator config = new Configurator();
        config.HiddenWindow = true;
        config.AudioQuality = AudioResampleQuality.HighHann;
        Engine.Setup(config);

        _audioAsset = Engine.AssetLoader.Get<AudioAsset>("pepsi.wav")!;
        _output = new float[_audioAsset.SoundData.Length * 4];
    }

    [Benchmark]
    public void FullResampleEmotion()
    {
        _audioAsset.AudioConverter.GetResampledFrames(_targetFormat, 0, _targetFormat.SecondsToFrames(_audioAsset.Duration), _output);
    }

    [Benchmark]
    public void FullResampleEmotionFast()
    {
        _audioAsset.AudioConverter.Optimized_GetResampledFrames(_targetFormat, 0, _targetFormat.SecondsToFrames(_audioAsset.Duration), _output);
    }

    [Benchmark]
    public void MediumResampleEmotion()
    {
        int size = _targetFormat.SecondsToFrames(_audioAsset.Duration) / 5;
        _audioAsset.AudioConverter.GetResampledFrames(_targetFormat, size, size, _output);
    }

    [Benchmark]
    public void MediumResampleEmotionFast()
    {
        int size = _targetFormat.SecondsToFrames(_audioAsset.Duration) / 5;
        _audioAsset.AudioConverter.Optimized_GetResampledFrames(_targetFormat, size, size, _output);
    }

    [Benchmark]
    public void SmallResampleEmotion()
    {
        int size = 2000;
        _audioAsset.AudioConverter.GetResampledFrames(_targetFormat, size, size, _output);
    }

    [Benchmark]
    public void SmallResampleEmotionFast()
    {
        int size = 2000;
        _audioAsset.AudioConverter.Optimized_GetResampledFrames(_targetFormat, size, size, _output);
    }

    [Benchmark]
    public void ContinousResampleEmotion()
    {
        int offset = 0;
        int totalSize = _targetFormat.SecondsToFrames(_audioAsset.Duration);
        int querySize = 2000;
        for (int i = 0; i < totalSize; i++)
        {
            offset += _audioAsset.AudioConverter.GetResampledFrames(_targetFormat, offset, querySize, _output);
        }
    }

    [Benchmark]
    public void ContinousResampleEmotionFast()
    {
        int offset = 0;
        int totalSize = _targetFormat.SecondsToFrames(_audioAsset.Duration);
        int querySize = 2000;
        for (int i = 0; i < totalSize; i++)
        {
            offset += _audioAsset.AudioConverter.Optimized_GetResampledFrames(_targetFormat, offset, querySize, _output);
        }
    }
}