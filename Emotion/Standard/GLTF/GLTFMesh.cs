#region Using

using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFMesh
{
    [JsonPropertyName("primitives")]
    public GLTFMeshPrimitives[] Primitives { get; set; }
}