﻿#region Using

using System.Text.Json.Serialization;

#endregion

#nullable enable

namespace Tests.EngineTests.SerializationTestsSupport.GLTF;

public class GLTFAnimation
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("samplers")]
    public GLTFAnimationSampler[] Samplers { get; set; }

    [JsonPropertyName("channels")]
    public GLTFAnimationChannel[] Channels { get; set; }
}