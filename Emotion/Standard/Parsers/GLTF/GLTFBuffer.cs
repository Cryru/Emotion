#nullable enable

using Emotion.Standard.Serialization.Json;

namespace Emotion.Standard.Parsers.GLTF;

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