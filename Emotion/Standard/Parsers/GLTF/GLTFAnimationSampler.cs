#nullable enable

namespace Emotion.Standard.Parsers.GLTF;

public class GLTFAnimationSampler
{
    public int Input { get; set; }

    public int Output { get; set; }

    public string? Interpolation { get; set; }
}