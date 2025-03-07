using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics;
using Emotion.Graphics.Shader;
using Emotion.Graphics.Shading;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Scenography;
using OpenGL;

namespace Emotion.ExecTest.ExamplesOne;

public class ShaderTestScene : SceneWithMap
{
    private NewShaderAsset _shaderAsset;

    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        _shaderAsset = Engine.AssetLoader.ONE_Get<NewShaderAsset>("MyShader.glsl");
        yield return _shaderAsset;
    }

    public override void RenderScene(RenderComposer c)
    {
        base.RenderScene(c);

        c.SetUseViewMatrix(false);

        c.SetShader(_shaderAsset.CompiledShader);

        c.RenderSprite(Vector3.Zero, new Vector2(512), Color.PrettyYellow);

        c.SetShader(null);
    }
}
