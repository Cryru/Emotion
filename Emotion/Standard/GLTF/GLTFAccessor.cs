#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFAccessor
{
    public string Name { get; set; } = string.Empty;

    public int BufferView { get; set; }

    public int ComponentType { get; set; }

    public int Count { get; set; }

    public string Type { get; set; } = string.Empty;

    public bool Normalized { get; set; }

    public int ByteOffset { get; set; }
}