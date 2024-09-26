#region Using

using BenchmarkDotNet.Attributes;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Image.ImgBin;
using Emotion.Standard.Image.PNG;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

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
            _fileBytes.Span.Md5GetStableHashCode();
            "random string".Md5GetStableHashCode();
        }
    }

    public static class Md5HashCode
    {
        public static int Md5GetStableHashCode(this string str)
        {
            ReadOnlySpan<byte> stringAsBytes = MemoryMarshal.Cast<char, byte>(str);
            return stringAsBytes.Md5GetStableHashCode();
        }

        public static int Md5GetStableHashCode(this ReadOnlySpan<byte> span)
        {
            Span<byte> hash = stackalloc byte[32];

            if (MD5.TryHashData(span, hash, out int bytesWritten))
                return BitConverter.ToInt32(hash.Slice(0, bytesWritten));

            return BitConverter.ToInt32(hash);
        }
    }

}