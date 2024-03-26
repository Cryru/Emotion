#region Using

using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Emotion.IO.MeshAssetTypes.GLTF;

public class GLTFMesh
{
    [JsonPropertyName("primitives")]
    public GLTFMeshPrimitives[] Primitives { get; set; }

    [JsonPropertyName("material")]
    public int Material { get; set; }
}