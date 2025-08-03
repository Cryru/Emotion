#nullable enable

using System.Runtime.InteropServices;

namespace Emotion.Standard.DataStructures;

public struct NonAllocByteReader
{
    public ReadOnlyMemory<byte> Data;
    public int Position;

    public NonAllocByteReader(ReadOnlyMemory<byte> memory)
    {
        Data = memory;
    }

    public void SkipBytes(int bytes)
    {
        for (int i = 0; i < bytes; i++)
        {
            ReadGeneric<byte>();
        }
    }

    public byte ReadByte()
    {
        return ReadGeneric<byte>();
    }

    public int ReadInt32()
    {
        return ReadGeneric<int>();
    }

    public float ReadSingle()
    {
        return ReadGeneric<float>();
    }

    public unsafe T ReadGeneric<T>() where T : unmanaged
    {
        var size = sizeof(T);
        var output = MemoryMarshal.Read<T>(Data.Slice(Position, size).Span);
        Position += size;
        return output;
    }
}