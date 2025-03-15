#region Using

using Emotion.Common.Serialization;

#endregion

#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFBuffer
{
    public string Uri { get; set; }

    public int ByteLength { get; set; }

    /// <summary>
    /// Populated at runtime during parsing.
    /// </summary>
    [DontSerialize]
    public ReadOnlyMemory<byte> Data;
}