#region Using

using Emotion.Common.Serialization;
using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Tests.EngineTests.SerializationTestsSupport.GLTF;

public class GLTFBuffer
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; }

    [JsonPropertyName("byteLength")]
    public int ByteLength { get; set; }
}