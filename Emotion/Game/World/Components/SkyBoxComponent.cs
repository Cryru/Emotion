#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Assets;

namespace Emotion.Game.World.Components;

public class SkyBoxComponent : MeshComponent
{
    private AssetOrObjectReferenceLifecycleSupport<TextureCubemapAsset, TextureCubemap> _cubeMap;
    private TextureCubemap? _texture;

    public SkyBoxComponent(CubeMapTextureReference cubeMapTexture)
    {
        AlwaysRender = true;
        _cubeMap = new AssetOrObjectReferenceLifecycleSupport<TextureCubemapAsset, TextureCubemap>(cubeMapTexture, OnTextureChanged);
    }

    public override Coroutine? Init(GameObject obj)
    {
        Coroutine? r = base.Init(obj);
        Coroutine? r2 = _cubeMap.Init();
        return Coroutine.CombineRoutines(r, r2);
    }

    public override void Done(GameObject obj)
    {
        base.Done(obj);
        _cubeMap.Done();
    }

    public Coroutine? SetCubeMapTexture(CubeMapTextureReference texture)
    {
        return _cubeMap.Set(texture);
    }

    private void OnTextureChanged(TextureCubemap? texture)
    {
        _texture = texture;
    }

    protected override void OnEntityChanged()
    {
        base.OnEntityChanged();
        MeshEntityMetaState renderState = RenderState!;
        renderState.CustomRenderState = new RenderState()
        {
            ShaderName = "Shaders3D/SkyboxShader.glsl",
            AlphaBlending = true
        };
    }

    public override void Render(Renderer r)
    {
        if (_texture == null) return;
        TextureCubemap.EnsureBound(_texture.Pointer);
        base.Render(r);
    }
}