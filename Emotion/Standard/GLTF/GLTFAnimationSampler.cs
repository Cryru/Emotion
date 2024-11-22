#region Using

using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFAnimationSampler
{
    [JsonPropertyName("input")]
    public int Input { get; set; }

    [JsonPropertyName("output")]
    public int Output { get; set; }

    [JsonPropertyName("interpolation")]
    public string? Interpolation { get; set; }
}