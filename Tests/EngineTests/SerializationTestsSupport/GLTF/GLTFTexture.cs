#region Using

using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Tests.EngineTests.SerializationTestsSupport.GLTF;

public class GLTFTexture
{
    [JsonPropertyName("source")]
    public int Source { get; set; }
}