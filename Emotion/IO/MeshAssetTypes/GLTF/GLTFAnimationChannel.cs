#region Using

using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Emotion.IO.MeshAssetTypes.GLTF;

public class GLTFAnimationChannel
{
    [JsonPropertyName("sampler")]
    public int Sampler { get; set; }

    public class GLTFAnimationChannelTarget
    {
        [JsonPropertyName("node")]
        public int Node { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }
    }

    [JsonPropertyName("target")]
    public GLTFAnimationChannelTarget Target { get; set; }
}
