#nullable enable

namespace Emotion.Standard.Parsers.GLTF;

public class GLTFAnimation
{
    public string Name { get; set; } = string.Empty;

    public GLTFAnimationSampler[] Samplers { get; set; } = Array.Empty<GLTFAnimationSampler>();

    public GLTFAnimationChannel[] Channels { get; set; } = Array.Empty<GLTFAnimationChannel>();
}