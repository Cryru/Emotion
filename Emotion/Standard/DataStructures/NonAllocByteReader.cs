#nullable enable

using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Emotion.Standard.DataStructures;

public struct NonAllocByteReader
{
    public ReadOnlyMemory<byte> Data;
    public int Position;
    public readonly int BytesLeft => Data.Length - Position;

    public NonAllocByteReader(ReadOnlyMemory<byte> memory)
    {
        Data = memory;
    }

    public void SkipBytes(int bytes)
    {
        Position += bytes;
    }

    public byte ReadByte()
    {
        ReadOnlySpan<byte> s = Data.Span.Slice(Position);
        Position++;
        return s[0];
    }

    public ReadOnlySpan<byte> ReadBytes(int count)
    {
        ReadOnlySpan<byte> s = Data.Span.Slice(Position, count);
        Position += count;
        return s;
    }

    public int ReadInt32()
    {
        return ReadGeneric<int>();
    }

    public int ReadInt32BE()
    {
        ReadOnlySpan<byte> p = ReadBytes(4);
        var n = BitConverter.ToInt32(p);
        return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(n) : n;
    }

    public float ReadSingle()
    {
        return ReadGeneric<float>();
    }

    public unsafe T ReadGeneric<T>() where T : unmanaged
    {
        int size = sizeof(T);
        T output = MemoryMarshal.Read<T>(Data.Slice(Position, size).Span);
        Position += size;
        return output;
    }
}