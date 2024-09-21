#region Using

using BenchmarkDotNet.Attributes;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Image.ImgBin;
using Emotion.Standard.Image.PNG;

#endregion

namespace Emotion.Benchmark
{
    [MemoryDiagnoser]
    public class HashingBenchmark
    {
        private ReadOnlyMemory<byte> _fileBytes;

        [GlobalSetup]
        public void GlobalSetup()
        {
            Configurator config = new Configurator();
            config.HiddenWindow = true;
            Engine.Setup(config);

            var asset = Engine.AssetLoader.Get<OtherAsset>("logoAlpha.png");
            _fileBytes = asset.Content;
        }

        [Benchmark]
        public void XXHash()
        {
            _fileBytes.Span.GetStableHashCode();
            "random string".GetStableHashCode();
        }
        
        [Benchmark]
        public void Md5()
        {
            _fileBytes.Span.__NoDepend_GetStableHashCode();
            "random string".__NoDepend_GetStableHashCode();
        }
    }
}