﻿#region Using

using Emotion.Editor;
using Emotion.Graphics.Assets;
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

    public SerializableAsset<TextureAsset>? DiffuseTextureName = null;

    public Texture DiffuseTexture = Texture.EmptyWhiteTexture;

    public RenderState State = RenderState.Default;

    public MeshMaterial()
    {
        State = RenderState.Default;
        //State.ShaderName = "Shaders3D/MeshShader.glsl";
        State.ShaderName = "Shaders/MeshShader.xml";
    }

    public static MeshMaterial DefaultMaterial = new MeshMaterial
    {
        Name = "Default"
    };

    public Texture GetDiffuseTexture()
    {
        if (DiffuseTextureName != null)
        {
            TextureAsset texture = Engine.AssetLoader.ONE_Get<TextureAsset>(DiffuseTextureName);
            if (texture.Loaded)
                return texture.Texture;
        }

        return DiffuseTexture;
    }
}