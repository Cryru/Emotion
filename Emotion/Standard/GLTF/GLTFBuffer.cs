#region Using

using Emotion.Common.Serialization;
using Emotion.Serialization.JSON;

#endregion

#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFBuffer : IJSONIndexOrNameReferencable
{
    public string? JSON_NAMED_ARRAY_ID { get; set; }

    public string Uri { get; set; } = string.Empty;

    public int ByteLength { get; set; }

    /// <summary>
    /// Populated at runtime during parsing.
    /// </summary>
    [DontSerialize]
    public ReadOnlyMemory<byte> Data;
}