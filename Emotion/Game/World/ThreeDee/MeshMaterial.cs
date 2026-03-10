#nullable enable

#region Using

using Emotion.Core.Systems.IO;
using Emotion.Graphics.Assets;
using Emotion.Graphics.Shader;

#endregion

namespace Emotion.Game.World.ThreeDee;

/// <summary>
/// Settings description of how to render a 3D object.
/// </summary>
public class MeshMaterial
{
    public string Name = string.Empty;

    public Color DiffuseColor = Color.White;

    public AssetObjectReference<TextureAsset, Texture> DiffuseTextureName = AssetObjectReference<TextureAsset, Texture>.Invalid;

    public Texture DiffuseTexture = Texture.EmptyWhiteTexture;

    public RenderState State;

    public MeshMaterial()
    {
        State = new RenderState
        {
            ShaderGroup = "Shaders3D/MeshShader.glsl"
        };
    }

    public static MeshMaterial DefaultMaterial = new MeshMaterial
    {
        Name = "Default"
    };

    public static MeshMaterial DefaultMaterialTwoSided = new MeshMaterial
    {
        Name = "Default",
        State =
        {
            FaceCulling = true
        }
    };

    public static MeshMaterial DefaultMaterialOneSided = new MeshMaterial
    {
        Name = "Default",
        State =
        {
            FaceCulling = false
        }
    };

    public Texture GetDiffuseTexture()
    {
        if (DiffuseTextureName.IsValid())
        {
            return DiffuseTextureName.GetObjectLoadinline() ?? Texture.EmptyWhiteTexture;
        }

        return DiffuseTexture;
    }

    public void EnsureAssetsLoaded()
    {
        // todo: use AssetOwner
        if (DiffuseTextureName.Type == AssetReferenceType.AssetName)
            Engine.AssetLoader.Get<TextureAsset>(DiffuseTextureName.AssetName, this);

        if (State.Shader.Type == AssetReferenceType.AssetName)
            Engine.AssetLoader.Get<ShaderAsset>(State.Shader.AssetName, this);

        State.ShaderGroup.ResolveAsset();
    }
}