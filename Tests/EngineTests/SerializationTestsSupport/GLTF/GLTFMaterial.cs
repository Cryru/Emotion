using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tests.EngineTests.SerializationTestsSupport.GLTF;

public class GLTFMaterial
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("emissiveFactor")]
    public float[] EmissiveFactor { get; set; }

    public class GLTFPBRMetallicRoughness
    {
        [JsonPropertyName("baseColorTexture")]
        public GLTFBaseColorTexture BaseColorTexture { get; set; }

        [JsonPropertyName("metallicFactor")]
        public float MetallicFactor { get; set; }
    }

    public class GLTFBaseColorTexture
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("texCoord")]
        public int TexCoord { get; set; }
    }

    [JsonPropertyName("pbrMetallicRoughness")]
    public GLTFPBRMetallicRoughness PBRMetallicRoughness { get; set; }
}