using Emotion.Serialization.JSON;

#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFMeshPrimitives
{
    public Dictionary<string, JSONArrayIndexOrName>? Attributes { get; set; }

    public JSONArrayIndexOrName Indices { get; set; }

    public JSONArrayIndexOrName Material { get; set; }
}