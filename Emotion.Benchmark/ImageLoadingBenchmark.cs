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
    public class ImageLoadingBenchmark
    {
        private ReadOnlyMemory<byte> _fileBytes;
        private ReadOnlyMemory<byte> _emotionFileBytes;

        [GlobalSetup]
        public void GlobalSetup()
        {
            Configurator config = new Configurator();
            config.HiddenWindow = true;
            Engine.Setup(config);

            var asset = Engine.AssetLoader.Get<OtherAsset>("logoAlpha.png");
            _fileBytes = asset.Content;

            var pixels = PngFormat.Decode(_fileBytes, out PngFileHeader header);
            _emotionFileBytes = ImgBinFormat.Encode(pixels, header.Size, header.PixelFormat);

            bool a = true;
        }

        [Benchmark]
        public void LoadPNG()
        {
            PngFormat.Decode(_fileBytes, out PngFileHeader header);
        }
        
        [Benchmark]
        public void LoadImgBin()
        {
            ImgBinFormat.Decode(_emotionFileBytes, out ImgBinFileHeader header);
        }
    }
}