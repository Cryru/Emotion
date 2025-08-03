#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Graphics.Assets;

namespace Emotion.Game.World.ThreeDee;

public class MapObjectSkybox : MapObjectMesh
{
    public SerializableAsset<TextureCubemapAsset> CubemapTexture = "";

    private TextureCubemapAsset? _loadedCubemapTexture;

    public MapObjectSkybox()
    {
        SetEntity(Cube.GetEntity());
        AlwaysRender = true;
    }

    public override void Init()
    {
        _loadedCubemapTexture = CubemapTexture.Get(this);

        MeshEntityMetaState renderState = RenderState!;
        renderState.CustomRenderState = new RenderState()
        {
            ShaderName = "Shaders3D/SkyboxShader.glsl",
            AlphaBlending = true
        };

        base.Init();
    }

    public override void Render(Renderer c)
    {
        if (_loadedCubemapTexture?.Texture == null) return;
        TextureCubemap.EnsureBound(_loadedCubemapTexture.Texture.Pointer);
        base.Render(c);
    }
}
