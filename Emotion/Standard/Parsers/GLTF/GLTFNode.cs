#nullable enable

namespace Emotion.Standard.Parsers.GLTF;

public class GLTFNode
{
    public string Name { get; set; } = string.Empty;

    public int[] Children { get; set; } = Array.Empty<int>();

    public float[]? Rotation { get; set; } = null;

    public float[]? Translation { get; set; } = null;

    public float[]? Scale { get; set; } = null;
}