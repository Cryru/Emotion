#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFMeshPrimitives
{
    public Dictionary<string, int>? Attributes { get; set; }

    public int Indices { get; set; }

    public int Material { get; set; }
}