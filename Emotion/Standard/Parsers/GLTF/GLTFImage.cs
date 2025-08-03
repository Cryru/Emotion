#nullable enable





#nullable enable

using Emotion.Standard.Serialization.Json;

namespace Emotion.Standard.Parsers.GLTF;

public class GLTFImage : IJSONIndexOrNameReferencable
{
    public string? JSON_NAMED_ARRAY_ID { get; set; }

    public string Uri { get; set; } = string.Empty;
}