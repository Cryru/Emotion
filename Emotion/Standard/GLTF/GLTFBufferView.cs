#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFBufferView
{
    public int Buffer { get; set; }

    public int ByteLength { get; set; }

    public int ByteOffset { get; set; }

    public int ByteStride { get; set; }

    public int Target { get; set; }
}
