#region Using

using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Emotion.IO.MeshAssetTypes.GLTF;


public class GLTFSkins
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("joints")]
    public int[] Joints { get; set; }

    [JsonPropertyName("inverseBindMatrices")]
    public int InverseBindMatrices { get; set; }
}