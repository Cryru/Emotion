using Emotion.Standard.DataStructures.OptimizedStringReadWrite;
using Emotion.Testing;
using System;

namespace Tests.EngineTests;

[Test]
[TestClassRunParallel]
public class MemoryWritingUtilitiesTest
{
    [Test]
    public void ValueStringWriter_UTF8()
    {
        Span<byte> writeHere = stackalloc byte[16];
        ValueStringWriter writer = new(writeHere);
        writer.WriteChar('A');
        writer.WriteChar('B');
        writer.WriteString("Hello!");
        writer.WriteNumber(1337);

        int bytesWritten = writer.BytesWritten;
        Assert.Equal(bytesWritten, sizeof(byte) + sizeof(byte) + sizeof(byte) * 6 + sizeof(int));

        Span<byte> expectedResult = [65, 66, 72, 101, 108, 108, 111, 33, 49, 51, 51, 55, 0, 0, 0, 0];
        Assert.True(writeHere.SequenceEqual(expectedResult));
    }
}
