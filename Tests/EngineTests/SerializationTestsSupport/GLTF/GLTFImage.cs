#region Using

using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Tests.EngineTests.SerializationTestsSupport.GLTF;

public class GLTFImage
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; }
}