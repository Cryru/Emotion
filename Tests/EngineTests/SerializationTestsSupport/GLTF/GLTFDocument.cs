#region Using

using System.Text.Json.Serialization;


#endregion

#nullable enable

namespace Tests.EngineTests.SerializationTestsSupport.GLTF;

public class GLTFDocument
{
    [JsonPropertyName("buffers")]
    public GLTFBuffer[] Buffers { get; set; }

    [JsonPropertyName("bufferViews")]
    public GLTFBufferView[] BufferViews { get; set; }

    [JsonPropertyName("images")]
    public GLTFImage[]? Images { get; set; }

    [JsonPropertyName("textures")]
    public GLTFTexture[] Textures { get; set; }

    [JsonPropertyName("accessors")]
    public GLTFAccessor[] Accessors { get; set; }

    [JsonPropertyName("animations")]
    public GLTFAnimation[] Animations { get; set; }

    [JsonPropertyName("meshes")]
    public GLTFMesh[] Meshes { get; set; }

    [JsonPropertyName("materials")]
    public GLTFMaterial[]? Materials { get; set; }

    [JsonPropertyName("skins")]
    public GLTFSkins[]? Skins { get; set; }

    [JsonPropertyName("nodes")]
    public GLTFNode[] Nodes { get; set; }
}