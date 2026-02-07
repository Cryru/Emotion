#region Using

using BenchmarkDotNet.Attributes;
using Emotion.Core;
using Emotion.Core.Systems.IO;
using Emotion.Standard.Parsers.Image.ImgBin;
using Emotion.Standard.Parsers.Image.PNG;

#endregion

namespace Emotion.Benchmark;

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
        Engine.StartHeadless(config);

        var asset = Engine.AssetLoader.Get<OtherAsset>("logoAlpha.png");
        _fileBytes = asset!.Content;

        var pixels = PngFormat.Decode(_fileBytes, out PngFileHeader header);
        _emotionFileBytes = ImgBinFormat.Encode(pixels, header.Size, header.PixelFormat);
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