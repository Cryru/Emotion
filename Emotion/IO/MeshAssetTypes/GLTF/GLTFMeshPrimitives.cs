#region Using

using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Emotion.IO.MeshAssetTypes.GLTF;

public class GLTFMeshPrimitives
{
    [JsonPropertyName("attributes")]
    public Dictionary<string, int> Attributes { get; set; }

    [JsonPropertyName("indices")]
    public int Indices { get; set; }
}