using Emotion.Serialization.JSON;

#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFTexture : IJSONIndexOrNameReferencable
{
    public string? JSON_NAMED_ARRAY_ID { get; set; }

    public JSONArrayIndexOrName Source { get; set; }

    public JSONArrayIndexOrName Sampler { get; set; }
}