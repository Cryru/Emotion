#region Using

using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFAccessor
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("bufferView")]
    public int BufferView { get; set; }

    [JsonPropertyName("componentType")]
    public int ComponentType { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("normalized")]
    public bool Normalized { get; set; }

    [JsonPropertyName("byteOffset")]
    public int ByteOffset { get; set; }
}