#nullable enable

using Emotion.Serialization.JSON;

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

public class GLTFMaterialPBRValues
{
    public float[] Ambient { get; set; } = Array.Empty<float>();

    public Color AmbientColor { get => ArrToColor(Ambient); }

    public JSONArrayIndexOrNameOrArrayOfFloats Diffuse { get; set; }

    public Color DiffuseColor { get => ArrToColor(Diffuse.ReferenceAsArrayOfFloats ?? Array.Empty<float>()); }

    public float[] Specular { get; set; } = Array.Empty<float>();

    public Color SpecularColor { get => ArrToColor(Specular); }

    public float[] Emission { get; set; } = Array.Empty<float>();

    public Color EmissionColor { get => ArrToColor(Emission); }

    private Color ArrToColor(float[] arr)
    {
        if (arr.Length < 4) return Color.White;
        return new Color(new Vector4(arr[0], arr[1], arr[2], arr[3]));
    }
}

public class GLTFMaterial : IJSONIndexOrNameReferencable
{
    public string? JSON_NAMED_ARRAY_ID { get; set; }

    public string Name { get; set; } = string.Empty;

    public float[] EmissiveFactor { get; set; } = Array.Empty<float>();

    // todo: attribute so we can name this actually PBR
    // in the json it is PbrMetallicRoughness
    public GLTFMaterialPBR? PbrMetallicRoughness { get; set; }

    public GLTFMaterialPBRValues? Values { get; set; }
}