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
    /// <summary>
    /// Whether the chunk is valid.
    /// </summary>
    public bool Valid;

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
        int bytesLeft = stream.BytesLeft;
        if (bytesLeft < 8)
        {
            Engine.Log.Warning($"Chunk header missing chunk length!", MessageSource.ImagePng);
            return;
        }

        int chunkLength = stream.ReadInt32BE();

        // Read chunk type
        Type.A = (char) stream.ReadByte();
        Type.B = (char) stream.ReadByte();
        Type.C = (char) stream.ReadByte();
        Type.D = (char) stream.ReadByte();

        ChunkOffset = stream.Position;
        ChunkLength = chunkLength;
        stream.SkipBytes(chunkLength);

        bytesLeft = stream.BytesLeft;
        if (bytesLeft < 4)
        {
            Engine.Log.Warning($"Chunk header missing compressed data header!", MessageSource.ImagePng);
            return;
        }
        stream.SkipBytes(4); // CRC - we don't care to check it :)
        Valid = true;
    }
}
