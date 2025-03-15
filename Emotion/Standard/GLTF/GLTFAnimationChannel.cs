#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFAnimationChannelTarget
{
    public int Node { get; set; }

    public string Path { get; set; } = string.Empty;
}

public class GLTFAnimationChannel
{
    public int Sampler { get; set; }

    public GLTFAnimationChannelTarget? Target { get; set; }
}
