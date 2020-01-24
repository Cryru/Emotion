#region Using

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Test;
using Tests.Results;

#endregion

namespace Tests.Classes
{
    [Test]
    public class ShaderTests
    {
        /// <summary>
        /// Test whether loading of shaders and drawing with them works.
        /// Also tests loading of shaders with vert and without frag.
        /// </summary>
        [Test]
        public void ShaderLoadAndDraw()
        {
            var shaders = new List<ShaderAsset>();

            // Which phase of the test is happening. Done to split the draw function.
            var shaderTest = 0;

            shaders.Add(Engine.AssetLoader.Get<ShaderAsset>("Shaders/TestShader.xml"));
            shaders.Add(Engine.AssetLoader.Get<ShaderAsset>("Shaders/TestShaderFragOnly.xml"));
            shaders.Add(Engine.AssetLoader.Get<ShaderAsset>("Shaders/TestShaderVertOnly.xml"));

            void Draw(string resultId)
            {
                Runner.ExecuteAsLoop(_ =>
                {
                    RenderComposer composer = Engine.Renderer.StartFrame();

                    // Set shader.
                    if (shaderTest != 0)
                        composer.SetShader(shaders[shaderTest - 1].Shader);

                    // Render a blank square.
                    composer.RenderSprite(new Vector3(10, 10, 0), new Vector2(10, 10), Color.White);

                    // Reset to default shader.
                    composer.SetShader();

                    Engine.Renderer.EndFrame();
                    Runner.VerifyScreenshot(resultId);
                }).WaitOne();
            }

            // Ensure all shaders are loaded.
            foreach (ShaderAsset s in shaders)
            {
                Assert.True(s.Shader != null);
                Assert.False(s.IsFallback);
            }

            // Change to phase 1. This is drawing with the test shader.
            Draw(ResultDb.ShaderTest0);
            shaderTest = 1;
            Draw(ResultDb.ShaderTest1);
            // Change to phase 2. This is drawing with the vert missing shader.
            shaderTest = 2;
            Draw(ResultDb.ShaderTest2);
            // Change to phase 3. This is drawing with the frag missing shader.
            shaderTest = 3;
            Draw(ResultDb.ShaderTest3);

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
        public void ShaderBrokenLoad()
        {
            var shader = Engine.AssetLoader.Get<ShaderAsset>("Shaders/BrokenShader.xml");
            ;

            Runner.ExecuteAsLoop(_ =>
            {
                RenderComposer composer = Engine.Renderer.StartFrame();

                // Set shader.
                composer.SetShader(shader.Shader);

                // Render a blank square.
                composer.RenderSprite(new Vector3(10, 10, 0), new Vector2(10, 10), Color.White);

                // Reset to default shader.
                composer.SetShader();

                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.BrokenShader);
            }).WaitOne();

            // Even though the shader is broken, and it doesn't specify a fallback, it should load the compat shader.
            Assert.True(shader.Shader != null);
            Assert.True(shader.IsFallback);

            Engine.AssetLoader.Destroy(shader.Name);
        }

        /// <summary>
        /// Test whether shader fallback works.
        /// </summary>
        [Test]
        public void ShaderFallback()
        {
            var shader = Engine.AssetLoader.Get<ShaderAsset>("Shaders/BrokenShaderWithFallback.xml");
            ;

            Runner.ExecuteAsLoop(_ =>
            {
                RenderComposer composer = Engine.Renderer.StartFrame();

                // Set shader.
                composer.SetShader(shader.Shader);

                // Render a blank square.
                composer.RenderSprite(new Vector3(10, 10, 0), new Vector2(10, 10), Color.White);

                // Reset to default shader.
                composer.SetShader();

                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.ShaderFallback);
            }).WaitOne();

            // The shader should've loaded its fallback.
            Assert.True(shader.Shader != null);
            Assert.True(shader.IsFallback);

            Engine.AssetLoader.Destroy(shader.Name);
        }
    }
}