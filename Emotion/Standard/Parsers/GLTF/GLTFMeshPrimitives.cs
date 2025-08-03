#nullable enable

using Emotion.Standard.Serialization.Json;

namespace Emotion.Standard.Parsers.GLTF;

public class GLTFMeshPrimitives
{
    public Dictionary<string, JSONArrayIndexOrName>? Attributes { get; set; }

    public JSONArrayIndexOrName Indices { get; set; }

    public JSONArrayIndexOrName Material { get; set; }
}