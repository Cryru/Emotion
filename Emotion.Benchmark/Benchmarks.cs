#region Using

using BenchmarkDotNet.Attributes;
using Emotion.Core;
using Emotion.Core.Systems.IO;
using Emotion.Graphics.Assets;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Standard.Memory;

#endregion

namespace Emotion.Benchmark;

[MemoryDiagnoser]
public class Benchmarks
{
    //private AudioTests.TestAudioContext.TestAudioLayer? _layer;
    private AudioAsset? _asset;

    private TextureAsset? _textureOne;
    private TextureAsset? _textureTwo;
    private TextureAtlas? _atlas;
    private int _atlasMemorySize;
    private IntPtr _atlasMemory;

    [GlobalSetup]
    public void GlobalSetup()
    {
        Configurator config = new Configurator();
        config.HiddenWindow = true;
        Engine.StartHeadless(config);

        //_asset = Engine.AssetLoader.Get<AudioAsset>("Audio/pepsi.wav");
        //var testAudio = new AudioTests.TestAudioContext(null);
        //_layer = (AudioTests.TestAudioContext.TestAudioLayer) testAudio.CreateLayer("Benchmark");
        //_layer.PlayNext(new AudioTrack(_asset)
        //{
        //    SetLoopingCurrent = true
        //});
        //_layer.Update(1); // To create test array.

        _textureOne = Engine.AssetLoader.Get<TextureAsset>("logoAlpha.png");
        _textureTwo = Engine.AssetLoader.Get<TextureAsset>("logoAsymmetric.png");
        _atlas = new TextureAtlas(false);//, 1);
        _atlas.TryBatchTexture(_textureOne?.Texture);
        _atlas.TryBatchTexture(_textureTwo?.Texture);

        _atlasMemorySize = VertexData.SizeInBytes * 1024;
        _atlasMemory = UnmanagedMemoryAllocator.MemAlloc(_atlasMemorySize);

        _atlas.Update(Engine.Renderer);
    }

    [Benchmark]
    public void AudioResample()
    {
        //_layer?.Update(1);
    }

    [Benchmark]
    public void Atlas()
    {
        if (_atlas == null) return;

        var toStruct = 0;
        for (var i = 0; i < 256; i += 2)
        {
            toStruct += 4;
            _atlas.RecordTextureMapping(_textureOne?.Texture, toStruct);
            toStruct += 4;
            _atlas.RecordTextureMapping(_textureTwo?.Texture, toStruct);
        }

        _atlas.RemapBatchUVs(_atlasMemory, (uint) _atlasMemorySize, (uint) VertexData.SizeInBytes, 12);
    }
}