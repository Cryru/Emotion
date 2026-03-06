#region Using

using Emotion.Core.Systems.IO;
using Emotion.Graphics.Assets;
using Emotion.Graphics.Shader;

#endregion

#nullable enable

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

    public RenderState State = RenderState.Default;

    public MeshMaterial()
    {
        State = RenderState.Default;
        //State.ShaderName = "Shaders3D/MeshShader.glsl";
        State.Shader = "Shaders/MeshShader.xml";
    }

    public static MeshMaterial DefaultMaterial = new MeshMaterial
    {
        Name = "Default"
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
    }
}