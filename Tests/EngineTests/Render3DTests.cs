#nullable enable

using Emotion.Core;
using Emotion.Game.World;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.Shader;
using Emotion.Testing;
using System.Collections;
using System.Numerics;

namespace Tests.EngineTests;

public class Render3DTests : TestingScene
{
    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        GameObject obj = Map.NewMeshObject("WoWModels/rabbit2/rabbit2_rabbitskin2_white.gltf");
        obj.Scale3D = new Vector3(100);
        yield break;
    }

    protected override void TestDraw(Renderer c)
    {
        Map.Render(c);
    }

    protected override void TestUpdate()
    {
        Map.Update(16);
    }

    [Test]
    public IEnumerator PipelineBasicBuild()
    {
        string basicShader = @"// INCLUDE_FILE <Shaders/Common.h>

// DEFINE_VERTEX_ATTRIBUTE Position V_Pos
// DEFINE_VERTEX_ATTRIBUTE UV V_UV

VERT_TO_FRAGMENT vec3 F_Pos;

#ifdef VERT_SHADER

vec4 VertexShaderMain()
{
    vec4 totalPosition = vec4(V_Pos, 1.0);
    vec4 localPos = modelMatrix * totalPosition;
    F_Pos = vec3(localPos);
    return projectionMatrix * viewMatrix * localPos;
}

#endif

#ifdef FRAG_SHADER

#define ALPHA_DISCARD 0.01

uniform Texture diffuseTexture;

vec4 FragmentShaderMain()
{
    vec4 textureColor = texture(diffuseTexture, V_UV);

    vec4 finalColor = textureColor;
    if (finalColor.a < ALPHA_DISCARD) discard;
    return finalColor;
}

#endif";
        ShaderGroup pipeline = new ShaderGroup("Test", basicShader);
        pipeline.VersionString = $"#version 330"; // Force for deterministic test
        yield return pipeline.Init();

        ShaderGroup.ShaderOut shaderOut = new ShaderGroup.ShaderOut();
        yield return pipeline.GetCompiledShaderForPipelineDefRoutine(shaderOut, new ShaderGroupDefinition(VertexData.Format));

        Assert.NotNull(shaderOut.OutShaderProgram);
    }

    [Test]
    public IEnumerator WorldWith2DCamera()
    {
        Camera3D cam = new Camera3D(Vector3.Zero);
        Engine.Renderer.Camera = cam;
        yield return new TestWaiterRunLoops(1);

        yield return ScreenshotPointFromAllSides(Vector3.Zero, nameof(WorldWith2DCamera));
    }

    #region Helpers

    public IEnumerator ScreenshotPointFromAllSides(Vector3 point, string funcName)
    {
        var cam = Engine.Renderer.Camera;

        cam.Position = point + new Vector3(0, 0, 100);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        yield return VerifyScreenshot(nameof(Render3DTests), funcName);

        cam.Position = point + new Vector3(0, 0, -100);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        yield return VerifyScreenshot(nameof(Render3DTests), funcName);

        cam.Position = point + new Vector3(100, 0, 0);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        yield return VerifyScreenshot(nameof(Render3DTests), funcName);

        cam.Position = point + new Vector3(-100, 0, 0);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        yield return VerifyScreenshot(nameof(Render3DTests), funcName);

        cam.Position = point + new Vector3(0, 100, 0);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        yield return VerifyScreenshot(nameof(Render3DTests), funcName);

        cam.Position = point + new Vector3(0, -100, 0);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        yield return VerifyScreenshot(nameof(Render3DTests), funcName);
    }

    #endregion
}
