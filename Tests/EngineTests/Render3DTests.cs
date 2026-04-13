#nullable enable

using Emotion.Core;
using Emotion.Game.World;
using Emotion.Game.World.Components;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.Shader;
using Emotion.Standard;
using Emotion.Testing;
using System.Collections;
using System.Numerics;

namespace Tests.EngineTests;

public class Render3DTests : TestingScene
{
    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
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
    public IEnumerator RenderGLTFModel()
    {
        GameObject obj = Map.NewMeshObject("WoWModels/rabbit2/rabbit2_rabbitskin2_white.gltf");
        obj.Scale3D = new Vector3(100);

        Camera3D cam = new Camera3D(Vector3.Zero);
        Engine.Renderer.Camera = cam;
        yield return new TestWaiterRunLoops(41); // Workaround to ensure the pipeline is loaded :/

        yield return ScreenshotPointFromAllSides(Vector3.Zero, nameof(RenderGLTFModel));
        Map.RemoveObject(obj);
    }

    [Test]
    public IEnumerator CubeRender()
    {
        GameObject obj = Map.NewMeshObject("");
        obj.Scale3D = new Vector3(50);

        Camera3D cam = new Camera3D(Vector3.Zero);
        Engine.Renderer.Camera = cam;
        yield return new TestWaiterRunLoops(41);

        yield return ScreenshotPointFromAllSides(Vector3.Zero, nameof(CubeRender));
        Map.RemoveObject(obj);
    }

    [Test]
    public IEnumerator SkeletalAnimationBlending()
    {
        GameObject obj = Map.NewMeshObject("WoWModels/humanmaleguard/humanmaleguard.gltf");
        obj.Scale3D = new Vector3(15);

        Camera3D cam = new Camera3D(Vector3.Zero);
        Engine.Renderer.Camera = cam;
        yield return new TestWaiterRunLoops(41);

        MeshComponent? mesh = obj.GetComponent<MeshComponent>();
        mesh.SetAnimation("Stand (ID 0 variation 0)", 0);
        mesh.Update(300);

        yield return ScreenshotPointFromAllSides(Vector3.Zero, nameof(SkeletalAnimationBlending), true);

        // Play an animation on layer 1 - should override layer 0
        mesh.SetAnimation("Attack1H (ID 17 variation 0)", 1, false);
        mesh.Update(300);

        yield return ScreenshotPointFromAllSides(Vector3.Zero, nameof(SkeletalAnimationBlending), true);

        // Since the attack animation is not looped it should run out eventually
        mesh.Update(100_000);

        yield return ScreenshotPointFromAllSides(Vector3.Zero, nameof(SkeletalAnimationBlending), true);

        // Start running with a crossfade from the stand
        mesh.SetAnimation("Run (ID 5 variation 0)", 0, true, 250);
        mesh.Update(100);

        yield return ScreenshotPointFromAllSides(Vector3.Zero, nameof(SkeletalAnimationBlending), true);

        mesh.Update(200);

        yield return ScreenshotPointFromAllSides(Vector3.Zero, nameof(SkeletalAnimationBlending), true);

        // Custom transform
        mesh.RenderState.SetCustomTransformForJoint(
            "bone_Root",
            Matrix4x4.CreateRotationY(Maths.DegreesToRadians(50.0f)),
            200
        );

        yield return ScreenshotPointFromAllSides(Vector3.Zero, nameof(SkeletalAnimationBlending), true);

        mesh.Update(100);

        yield return ScreenshotPointFromAllSides(Vector3.Zero, nameof(SkeletalAnimationBlending), true);

        mesh.RenderState.SetCustomTransformForJoint(
           "bone_Root",
           Matrix4x4.CreateRotationY(-Maths.DegreesToRadians(50.0f)),
           200
        );
        mesh.Update(10);

        yield return ScreenshotPointFromAllSides(Vector3.Zero, nameof(SkeletalAnimationBlending), true);

        mesh.SetAnimation("Stand (ID 0 variation 0)", 0);
        mesh.Update(100);

        yield return ScreenshotPointFromAllSides(Vector3.Zero, nameof(SkeletalAnimationBlending), true);

        mesh.Update(300);

        yield return ScreenshotPointFromAllSides(Vector3.Zero, nameof(SkeletalAnimationBlending), true);

        Map.RemoveObject(obj);
    }

    #region Helpers

    public IEnumerator ScreenshotPointFromAllSides(Vector3 point, string funcName, bool skipTopAndBottom = false)
    {
        var cam = Engine.Renderer.Camera;

        if (!skipTopAndBottom)
        {
            cam.Position = point + new Vector3(0, 0, 100);
            cam.LookAtPoint(Vector3.Zero);

            yield return new TestWaiterRunLoops(1);
            yield return VerifyScreenshot(nameof(Render3DTests), funcName);

            cam.Position = point + new Vector3(0, 0, -100);
            cam.LookAtPoint(Vector3.Zero);

            yield return new TestWaiterRunLoops(1);
            yield return VerifyScreenshot(nameof(Render3DTests), funcName);
        }

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
