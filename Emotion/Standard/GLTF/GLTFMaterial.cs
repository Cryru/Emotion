#nullable enable

namespace Emotion.Standard.GLTF;

public class GLTFMaterialPBR
{
    public GLTFBaseColorTexture? BaseColorTexture { get; set; }

    public float MetallicFactor { get; set; }
}

public class GLTFBaseColorTexture
{
    public int Index { get; set; }

    public int TexCoord { get; set; }
}

public class GLTFMaterial
{
    public string Name { get; set; } = string.Empty;

    public float[] EmissiveFactor { get; set; } = Array.Empty<float>();

    public GLTFMaterialPBR? PBRMetallicRoughness { get; set; }
}