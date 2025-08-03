#nullable enable

namespace Emotion.Standard.Parsers.GLTF;

public class GLTFMesh
{
    public string Name { get; set; } = string.Empty;

    public GLTFMeshPrimitives[] Primitives { get; set; } = Array.Empty<GLTFMeshPrimitives>();
}