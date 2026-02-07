#region Using

using BenchmarkDotNet.Attributes;
using Emotion.Core;
using Emotion.Standard;
using Emotion.Standard.Parsers.GLTF;
using Emotion.Standard.Parsers.XML;
using Emotion.Standard.Serialization.Json;
using Emotion.Standard.Serialization.XML;
using Emotion.Testing;

#endregion

namespace Emotion.Benchmark;

public class TestClassWithPrimitiveMember
{
    public int Number;
}

[MemoryDiagnoser]
public class ReflectorBenchmark
{
    TestClassWithPrimitiveMember _classWithNumberInstance = new TestClassWithPrimitiveMember()
    {
        Number = 10
    };

    ReadOnlyMemory<byte> exampleFileData;
    ReadOnlyMemory<char> exampleFileDataUTF16;

    [GlobalSetup]
    public void GlobalSetup()
    {
        Configurator config = new Configurator();
        config.HiddenWindow = true;
        Engine.StartHeadless(config);

        exampleFileData = File.ReadAllBytes(Path.Join("Assets", "example.gltf"));

        var encodingGuess = Helpers.GuessStringEncoding(exampleFileData.Span);
        char[]? chars = null;
        if (encodingGuess.EncodingName == "Unicode (UTF-8)")
        {
            var decoder = encodingGuess.GetDecoder();
            int charCount = decoder.GetCharCount(exampleFileData.Span, true);
            chars = new char[charCount];
            decoder.GetChars(exampleFileData.Span, chars, true);
        }
        exampleFileDataUTF16 = chars;
    }

    [Benchmark]
    public GLTFDocument? Serialize_Emotion_UTF16()
    {
        return JSONSerialization.From<GLTFDocument>(exampleFileDataUTF16.Span);
    }

    [Benchmark]
    public GLTFDocument? Serialize_Emotion_UTF8()
    {
        return JSONSerialization.From<GLTFDocument>(exampleFileData.Span);
    }

    [Benchmark]
    public string ReflectorXML_ComplexWithPrimitiveMember()
    {
        return XMLSerialization.To(_classWithNumberInstance);
    }

    [Benchmark]
    public void ReflectorXML_ComplexWithPrimitiveMember_StackUTF16()
    {
        Span<char> utf16Text = stackalloc char[128];
        int bytesWrittenUtf16 = XMLSerialization.To(_classWithNumberInstance, new XMLConfig(), utf16Text);
        Assert.True(bytesWrittenUtf16 != -1); // Success
    }

    [Benchmark]
    public void ReflectorXML_ComplexWithPrimitiveMember_StackUTF8()
    {
        Span<byte> utf8Text = stackalloc byte[128];
        int bytesWrittenUtf8 = XMLSerialization.To(_classWithNumberInstance, new XMLConfig(), utf8Text);
        Assert.True(bytesWrittenUtf8 != -1); // Success
    }

    [Benchmark]
    public string StockEmotionXML_ComplexWithPrimitiveMember()
    {
        return XMLFormat.To(_classWithNumberInstance);
    }
}