#region Using

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.IO;
using Emotion.Utility;
using Tests.Classes;

#endregion

namespace Emotion.Benchmark
{
    [MemoryDiagnoser]
    public class Benchmarks
    {
        private AudioTests.TestAudioContext.TestAudioLayer? _layer;
        private AudioAsset? _asset;

        private TextureAsset? _textureOne;
        private TextureAsset? _textureTwo;
        private TextureAtlas? _atlas;
        private int _atlasMemorySize;
        private IntPtr _atlasMemory;

        [GlobalSetup]
        public void GlobalSetup()
        {
            Engine.Setup();
            _asset = Engine.AssetLoader.Get<AudioAsset>("Audio/pepsi.wav");
            var testAudio = new AudioTests.TestAudioContext(null);
            _layer = (AudioTests.TestAudioContext.TestAudioLayer) testAudio.CreateLayer("Benchmark");
            _layer.PlayNext(new AudioTrack(_asset)
            {
                SetLoopingCurrent = true
            });
            _layer.Update(1); // To create test array.

            _textureOne = Engine.AssetLoader.Get<TextureAsset>("logoAlpha.png");
            _textureTwo = Engine.AssetLoader.Get<TextureAsset>("logoAsymmetric.png");
            _atlas = new TextureAtlas(false, 1);
            _atlas.TryBatchTexture(_textureOne?.Texture);
            _atlas.TryBatchTexture(_textureTwo?.Texture);

            _atlasMemorySize = VertexData.SizeInBytes * 1024;
            _atlasMemory = UnmanagedMemoryAllocator.MemAlloc(_atlasMemorySize);

            _atlas.Update(Engine.Renderer);
        }

        [Benchmark]
        public void AudioResample()
        {
            _layer?.Update(1);
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

        private int DoWork(string a, string b, string c)
        {
            a = a + b + c;
            return a.Length;
        }

        [Benchmark]
        public void ThreadInvocationScheduleOld()
        {
            var resp = 0;
            GLThread.ExecuteGLThread(() =>
            {
                resp = DoWork("adsad", "gegwe", "hhrhr");
            });
            if (resp != 15) throw new Exception("whaa");
        }

        [Benchmark]
        public void ThreadInvocationScheduleNew()
        {
            int resp = GLThread.ExecuteGLThread(DoWork, "adsad", "gegwe", "hhrhr");
            if (resp != 15) throw new Exception("whaa");
        }
    }

    public class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<Benchmarks>();

            //var benchmark = new Benchmarks();
            //benchmark.GlobalSetup();
            //benchmark.Atlas();
        }
    }
}