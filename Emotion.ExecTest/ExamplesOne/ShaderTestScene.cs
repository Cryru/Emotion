using System.Collections;
using System.Numerics;
using Emotion.Core;
using Emotion.Core.Systems.Scenography;
using Emotion.Graphics;
using Emotion.Graphics.Shader;
using Emotion.Primitives;

namespace Emotion.ExecTest.ExamplesOne;

public class ShaderTestScene : SceneWithMap
{
    private NewShaderAsset _shaderAsset;

    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        _shaderAsset = Engine.AssetLoader.Get<NewShaderAsset>("MyShader.glsl");
        yield return _shaderAsset;
    }

    public override void RenderScene(Renderer c)
    {
        base.RenderScene(c);

        c.SetUseViewMatrix(false);

        c.SetShader(_shaderAsset.CompiledShader);

        c.RenderSprite(Vector3.Zero, new Vector2(512), Color.PrettyYellow);

        c.SetShader(null);
    }
}
