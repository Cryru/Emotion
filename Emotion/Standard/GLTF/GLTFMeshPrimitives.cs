#region Using

using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFMeshPrimitives
{
    [JsonPropertyName("attributes")]
    public Dictionary<string, int> Attributes { get; set; }

    [JsonPropertyName("indices")]
    public int Indices { get; set; }

    [JsonPropertyName("material")]
    public int Material { get; set; }
}