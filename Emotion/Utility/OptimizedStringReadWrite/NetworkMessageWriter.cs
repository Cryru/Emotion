#nullable enable

using Emotion;
using System.Buffers.Binary;
using System.Text;

namespace Emotion.Utility.OptimizedStringReadWrite;

public ref struct NetworkMessageWriter
{
    public int BytesWritten { get; set; } = 0;

    private Span<byte> _memory;

    public NetworkMessageWriter(Span<byte> memory)
    {
        _memory = memory;
    }

    public void WriteByte(byte bite)
    {
        Span<byte> memory = _memory.Slice(BytesWritten);
        memory[0] = bite;
        BytesWritten++;
    }

    public void WriteString(ReadOnlySpan<char> str)
    {
        Span<byte> stringAsAscii = stackalloc byte[str.Length];
        int stringAsciiByteLength = Encoding.ASCII.GetBytes(str, stringAsAscii);
        stringAsAscii = stringAsAscii.Slice(0, stringAsciiByteLength);

        Span<byte> memory = _memory.Slice(BytesWritten);
        BinaryPrimitives.WriteInt32LittleEndian(memory, stringAsciiByteLength);
        memory = memory.Slice(sizeof(int));

        stringAsAscii.CopyTo(memory);
        BytesWritten += sizeof(int) + stringAsciiByteLength;
    }

    public void WriteInt(int number)
    {
        Span<byte> memory = _memory.Slice(BytesWritten);
        BinaryPrimitives.WriteInt32LittleEndian(memory, number);
        BytesWritten += sizeof(int);
    }
}
