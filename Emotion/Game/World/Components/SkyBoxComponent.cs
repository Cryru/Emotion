#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Assets;

namespace Emotion.Game.World.Components;

public class SkyBoxComponent : MeshComponent
{
    private AssetOwner<TextureCubemapAsset, TextureCubemap> _cubeTextureOwner = new();
    private TextureCubemap? _texture;

    public SkyBoxComponent(CubeMapTextureReference cubeMapTexture) : base()
    {
        _cubeTextureOwner = new();
        _cubeTextureOwner.SetOnChangeCallback(static (_, component) => (component as SkyBoxComponent)?.OnTextureChanged(), this);
        _cubeTextureOwner.Set(cubeMapTexture);
    }

    public override Coroutine? Init(GameObject obj)
    {
        Coroutine? r = base.Init(obj);
        Coroutine? r2 = _cubeTextureOwner.GetCurrentLoading();
        return Coroutine.CombineRoutines(r, r2);
    }

    public override void Done(GameObject obj)
    {
        base.Done(obj);
        _cubeTextureOwner.Done();
    }

    public Coroutine? SetCubeMapTexture(CubeMapTextureReference texture)
    {
        return _cubeTextureOwner.Set(texture);
    }

    protected void OnTextureChanged()
    {
        TextureCubemap? texture = _cubeTextureOwner.GetCurrentObject();
        if (texture != null) // todo: component property?
            texture.Smooth = true;
        _texture = texture;
    }

    protected override void OnEntityChanged()
    {
        base.OnEntityChanged();
        MeshEntityMetaState renderState = RenderState!;
        renderState.CustomRenderState = new RenderState()
        {
            Shader = "Shaders3D/SkyboxShader.glsl",
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