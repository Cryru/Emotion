#region Using

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Emotion.Common;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Testing;

#endregion

namespace Tests.EngineTests;

public class ShaderTests : ProxyRenderTestingScene
{
    /// <summary>
    /// Test whether loading of shaders and drawing with them works.
    /// Also tests loading of shaders with vert and without frag.
    /// </summary>
    [Test]
    public IEnumerator ShaderTest()
    {
        var shaders = new List<ShaderAsset>();

        // Which phase of the test is happening. Done to split the draw function.
        var shaderTest = 0;

        shaders.Add(Engine.AssetLoader.Get<ShaderAsset>("Shaders/TestShader.xml"));
        shaders.Add(Engine.AssetLoader.Get<ShaderAsset>("Shaders/TestShaderFragOnly.xml"));
        shaders.Add(Engine.AssetLoader.Get<ShaderAsset>("Shaders/TestShaderVertOnly.xml"));

        // Ensure all shaders are loaded.
        foreach (ShaderAsset s in shaders)
        {
            Assert.True(s.Shader != null);
            Assert.False(s.IsFallback);
        }

        ToRender = (composer) =>
        {
            composer.SetUseViewMatrix(false);

            // Set shader.
            if (shaderTest != 0)
                composer.SetShader(shaders[shaderTest - 1].Shader);

            // Render a blank square.
            composer.RenderSprite(new Vector3(10, 10, 0), new Vector2(10, 10), Color.White);

            // Reset to default shader.
            composer.SetShader();
        };

        // Change to phase 1. This is drawing with the test shader.
        shaderTest = 0;
        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(ShaderTests), nameof(ShaderTest));

        shaderTest = 1;
        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(ShaderTests), nameof(ShaderTest));

        // Change to phase 2. This is drawing with the vert missing shader.
        shaderTest = 2;
        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(ShaderTests), nameof(ShaderTest));

        // Change to phase 3. This is drawing with the frag missing shader.
        shaderTest = 3;
        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(ShaderTests), nameof(ShaderTest));

        // Cleanup
        foreach (ShaderAsset s in shaders)
        {
            Engine.AssetLoader.Destroy(s.Name);
        }

        // Ensure the shaders are unloaded.
        Assert.Equal(shaders.Count, shaders.Select(x => x.Name).Except(Engine.AssetLoader.LoadedAssets.Select(x => x.Name)).Count());
    }

    /// <summary>
    /// Test whether loading of broken shaders behaves as expected.
    /// </summary>
    [Test]
    public IEnumerator ShaderBrokenLoad()
    {
        var shader = Engine.AssetLoader.Get<ShaderAsset>("Shaders/BrokenShader.xml");

        ToRender = (composer) =>
        {
            composer.SetUseViewMatrix(false);

            // Set shader.
            composer.SetShader(shader.Shader);

            // Render a blank square.
            composer.RenderSprite(new Vector3(10, 10, 0), new Vector2(10, 10), Color.White);

            // Reset to default shader.
            composer.SetShader();
        };

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(ShaderTests), nameof(ShaderBrokenLoad));

        // Even though the shader is broken, and it doesn't specify a fallback, it should load the compat shader.
        Assert.True(shader.Shader != null);
        Assert.True(shader.IsFallback);

        Engine.AssetLoader.Destroy(shader.Name);
    }

    /// <summary>
    /// Test whether shader fallback works.
    /// </summary>
    [Test]
    public IEnumerator ShaderFallback()
    {
        var shader = Engine.AssetLoader.Get<ShaderAsset>("Shaders/BrokenShaderWithFallback.xml");

        ToRender = (composer) =>
        {
            composer.SetUseViewMatrix(false);

            // Set shader.
            composer.SetShader(shader.Shader);

            // Render a blank square.
            composer.RenderSprite(new Vector3(10, 10, 0), new Vector2(10, 10), Color.White);

            // Reset to default shader.
            composer.SetShader();
        };

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(ShaderTests), nameof(ShaderFallback));

        // The shader should've loaded its fallback.
        Assert.True(shader.Shader != null);
        Assert.True(shader.IsFallback);

        Engine.AssetLoader.Destroy(shader.Name);
    }
}