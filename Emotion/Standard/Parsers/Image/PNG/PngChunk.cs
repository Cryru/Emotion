#nullable enable

namespace Emotion.Standard.Parsers.Image.PNG;

public struct PngChunkType
{
    public char A;
    public char B;
    public char C;
    public char D;

    public readonly bool Is(string type)
    {
        return type.Length == 4 &&
               A == type[0] &&
               B == type[1] &&
               C == type[2] &&
               D == type[3];
    }
}

/// <summary>
/// Stores header information about a chunk.
/// </summary>
public struct PngChunk
{
    public PngChunkType Type;

    /// <summary>
    /// Where the chunk data starts in the file
    /// </summary>
    public int ChunkOffset;

    /// <summary>
    /// The length of the chunk data
    /// </summary>
    public int ChunkLength;

    public PngChunk(ref NonAllocByteReader stream)
    {
        int chunkLength = stream.ReadInt32BE();

        // Read chunk type
        Type.A = (char) stream.ReadByte();
        Type.B = (char) stream.ReadByte();
        Type.C = (char) stream.ReadByte();
        Type.D = (char) stream.ReadByte();

        ChunkOffset = stream.Position;
        ChunkLength = chunkLength;
        stream.SkipBytes(chunkLength);
        stream.SkipBytes(4); // CRC - we don't care to check it :)
    }
}
