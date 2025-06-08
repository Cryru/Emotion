#region Using

using Emotion.Editor;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shader;
using Emotion.IO;

#endregion

#nullable enable

namespace Emotion.Graphics.ThreeDee;

/// <summary>
/// Settings description of how to render a 3D object.
/// </summary>
public class MeshMaterial
{
    public string Name = string.Empty;

    public Color DiffuseColor = Color.White;

    [AssetFileName<TextureAsset>]
    public string? DiffuseTextureName = null;

    public Texture DiffuseTexture = Texture.EmptyWhiteTexture;

    public bool BackFaceCulling = true;

    public SerializableAsset<NewShaderAsset>? Shader = null;

    public RenderState State = RenderState.Default;

    public static MeshMaterial DefaultMaterial = new MeshMaterial
    {
        Name = "Default"
    };
}