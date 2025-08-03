#nullable enable

using Emotion.Standard.Serialization.Json;

namespace Emotion.Standard.Parsers.GLTF;

public class GLTFTexture : IJSONIndexOrNameReferencable
{
    public string? JSON_NAMED_ARRAY_ID { get; set; }

    public JSONArrayIndexOrName Source { get; set; }

    public JSONArrayIndexOrName Sampler { get; set; }
}