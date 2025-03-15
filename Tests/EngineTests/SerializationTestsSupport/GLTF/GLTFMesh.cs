#region Using

using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Tests.EngineTests.SerializationTestsSupport.GLTF;

public class GLTFMesh
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("primitives")]
    public GLTFMeshPrimitives[] Primitives { get; set; }
}