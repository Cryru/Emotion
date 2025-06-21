#nullable enable

using Emotion.Serialization.JSON;

namespace Emotion.Standard.GLTF;

public class GLTFImage : IJSONIndexOrNameReferencable
{
    public string? JSON_NAMED_ARRAY_ID { get; set; }

    public string Uri { get; set; } = string.Empty;
}