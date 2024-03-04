#region Using

using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Emotion.IO.MeshAssetTypes.GLTF;

public class GLTFAnimationSampler
{
    [JsonPropertyName("input")]
    public int Input { get; set; }

    [JsonPropertyName("output")]
    public int Output { get; set; }
}