#region Using

using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Emotion.IO.MeshAssetTypes.GLTF;

public class GLTFBufferView
{
    [JsonPropertyName("buffer")]
    public int Buffer { get; set; }

    [JsonPropertyName("byteLength")]
    public int ByteLength { get; set; }

    [JsonPropertyName("byteOffset")]
    public int ByteOffset { get; set; }

    [JsonPropertyName("byteStride")]
    public int ByteStride { get; set; }

    [JsonPropertyName("target")]
    public int Target { get; set; }
}
