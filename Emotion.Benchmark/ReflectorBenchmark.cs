#region Using

using BenchmarkDotNet.Attributes;
using Emotion.Common;
using Emotion.IO;
using Emotion.Serialization.XML;
using Emotion.Standard.XML;
using Emotion.Testing;
using FastSerialization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

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

    [GlobalSetup]
    public void GlobalSetup()
    {
        Configurator config = new Configurator();
        config.HiddenWindow = true;
        Engine.Setup(config);
    }

    [Benchmark]
    public string ReflectorXML_ComplexWithPrimitiveMember()
    {
        return XMLSerialize.To(_classWithNumberInstance);
    }

    [Benchmark]
    public void ReflectorXML_ComplexWithPrimitiveMember_StackUTF16()
    {
        Span<char> utf16Text = stackalloc char[128];
        int bytesWrittenUtf16 = XMLSerialize.To(_classWithNumberInstance, new XMLConfig(), utf16Text);
        Assert.True(bytesWrittenUtf16 != -1); // Success
    }

    [Benchmark]
    public void ReflectorXML_ComplexWithPrimitiveMember_StackUTF8()
    {
        Span<byte> utf8Text = stackalloc byte[128];
        int bytesWrittenUtf8 = XMLSerialize.To(_classWithNumberInstance, new XMLConfig(), utf8Text);
        Assert.True(bytesWrittenUtf8 != -1); // Success
    }

    [Benchmark]
    public string StockEmotionXML_ComplexWithPrimitiveMember()
    {
        return XMLFormat.To(_classWithNumberInstance);
    }
}