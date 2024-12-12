#region Using

using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Emotion.IO.MeshAssetTypes.Assimp.GLTF;

public class GLTFNode
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("rotation")]
    public float[] Rotation { get; set; }

    [JsonPropertyName("translation")]
    public float[] Translation { get; set; }

    [JsonPropertyName("scale")]
    public float[] Scale { get; set; }
}