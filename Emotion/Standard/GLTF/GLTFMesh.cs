#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFMesh
{
    public string Name { get; set; } = string.Empty;

    public GLTFMeshPrimitives[] Primitives { get; set; } = Array.Empty<GLTFMeshPrimitives>();
}