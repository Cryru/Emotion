#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFSkins
{
    public string Name { get; set; } = string.Empty;

    public int[] Joints { get; set; } = Array.Empty<int>();

    public int InverseBindMatrices { get; set; }
}