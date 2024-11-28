#region Using

using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFBuffer
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; }

    [JsonPropertyName("byteLength")]
    public int ByteLength { get; set; }

    [JsonIgnore]
    public ReadOnlyMemory<byte> Data;
}