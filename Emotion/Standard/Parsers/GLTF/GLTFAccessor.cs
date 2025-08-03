#nullable enable




#nullable enable

using Emotion.Standard.Serialization.Json;

namespace Emotion.Standard.Parsers.GLTF;

public class GLTFAccessor : IJSONIndexOrNameReferencable
{
    public string? JSON_NAMED_ARRAY_ID { get; set; }

    public string Name { get; set; } = string.Empty;

    public JSONArrayIndexOrName BufferView { get; set; }

    public int ComponentType { get; set; }

    public int Count { get; set; }

    public string Type { get; set; } = string.Empty;

    public bool Normalized { get; set; }

    public int ByteOffset { get; set; }
}