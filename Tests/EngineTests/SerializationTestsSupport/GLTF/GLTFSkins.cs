#region Using

using System;
using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Tests.EngineTests.SerializationTestsSupport.GLTF;

public class GLTFSkins
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("joints")]
    public int[] Joints { get; set; } = Array.Empty<int>();

    [JsonPropertyName("inverseBindMatrices")]
    public int InverseBindMatrices { get; set; }
}