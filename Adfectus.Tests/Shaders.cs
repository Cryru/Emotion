#region Using

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Adfectus.Common;
using Adfectus.IO;
using Adfectus.Primitives;
using Adfectus.Tests.Scenes;
using Xunit;

#endregion

namespace Adfectus.Tests
{
    /// <summary>
    /// Tests related to testing the shader functionality of Emotion.
    /// </summary>
    [Collection("main")]
    public class Shaders
    {
        /// <summary>
        /// Test whether loading of shaders and drawing with them works.
        /// Also tests loading of shaders with vert and without frag.
        /// </summary>
        [Fact]
        public void ShaderLoadAndDraw()
        {
            List<ShaderAsset> shaders = new List<ShaderAsset>();

            // Which phase of the test is happening. Done to split the draw function.
            int shaderTest = 0;

            // Create scene for this test.
            TestScene extScene = new TestScene
            {
                // Load a shader. Also tests whether loading of shaders within another thread works.
                ExtLoad = () =>
                {
                    shaders.Add(Engine.AssetLoader.Get<ShaderAsset>("Shaders/TestShader.xml"));
                    shaders.Add(Engine.AssetLoader.Get<ShaderAsset>("Shaders/TestShaderFragOnly.xml"));
                    shaders.Add(Engine.AssetLoader.Get<ShaderAsset>("Shaders/TestShaderVertOnly.xml"));
                },
                // Until will unload the shaders.
                ExtUnload = () =>
                {
                    foreach (ShaderAsset s in shaders)
                    {
                        Engine.AssetLoader.Destroy(s.Name);
                    }
                },
                // Draw all textures in a grid.
                ExtDraw = () =>
                {
                    // Set shader.
                    if (shaderTest != 0)
                        Engine.Renderer.SetShader(shaders[shaderTest - 1].Shader);

                    // Render a blank square.
                    Engine.Renderer.Render(new Vector3(10, 10, 0), new Vector2(10, 10), Color.White);

                    // Reset to default shader.
                    Engine.Renderer.SetShader();
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Ensure all shaders are loaded.
            foreach (ShaderAsset s in shaders)
            {
                Assert.NotNull(s.Shader);
                Assert.False(s.IsFallback);
            }

            // Check if what is currently on screen is what is expected.
            Assert.Equal("hC87cJBgdgR51XN76cIe14vmU1vlUWXgN3xxrp8WOdM=", Helpers.TakeScreenshot());
            //Helpers.TakeScreenshot();
            // Change to phase 1. This is drawing with the test shader.
            shaderTest = 1;
            extScene.WaitFrames(2).Wait();
            Assert.Equal("PKlb8X5gkVtCWie3LIKpcYzEc5nNjr2Xrz/gCqs54yA=", Helpers.TakeScreenshot());
            //Helpers.TakeScreenshot();
            // Change to phase 2. This is drawing with the vert missing shader.
            shaderTest = 2;
            extScene.WaitFrames(2).Wait();
            Assert.Equal("wPWltXJb1nv7uc9zvVZ9ks+sKvLXRoUw1wfjL7iBo9I=", Helpers.TakeScreenshot());
            //Helpers.TakeScreenshot();
            // Change to phase 3. This is drawing with the frag missing shader.
            shaderTest = 3;
            extScene.WaitFrames(2).Wait();
            Assert.Equal("1eZu/aah1RHU1dFOPkTVWwErYUdKwfCaBUC83uLUaGo=", Helpers.TakeScreenshot());
            //Helpers.TakeScreenshot();
            // Cleanup.
            Helpers.UnloadScene();

            // Ensure the shaders are unloaded.
            Assert.Equal(shaders.Count, shaders.Select(x => x.Name).Except(Engine.AssetLoader.LoadedAssets.Select(x => x.Name)).Count());
        }

        /// <summary>
        /// Test whether loading of broken shaders behaves as expected.
        /// </summary>
        [Fact]
        public void ShaderBrokenLoad()
        {
            ShaderAsset shader = null;

            // Create scene for this test.
            TestScene extScene = new TestScene
            {
                // Load a shader. Also tests whether loading of shaders within another thread works.
                ExtLoad = () => { shader = Engine.AssetLoader.Get<ShaderAsset>("Shaders/BrokenShader.xml"); },
                // Until will unload the shaders.
                ExtUnload = () => { Engine.AssetLoader.Destroy(shader.Name); },
                // Draw all textures in a grid.
                ExtDraw = () =>
                {
                    Engine.Renderer.SetShader(shader.Shader);

                    // Render a blank square.
                    Engine.Renderer.Render(new Vector3(10, 10, 0), new Vector2(10, 10), Color.White);

                    // Reset to default shader.
                    Engine.Renderer.SetShader();
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Even though the shader is broken, and it doesn't specify a fallback, it should load the compat shader.
            Assert.NotNull(shader.Shader);
            Assert.True(shader.IsFallback);

            // Check if what is currently on screen is what is expected.
            Assert.Equal("hC87cJBgdgR51XN76cIe14vmU1vlUWXgN3xxrp8WOdM=", Helpers.TakeScreenshot());

            // Cleanup.
            Helpers.UnloadScene();

            // Ensure the shaders are unloaded.
            Assert.DoesNotContain(shader.Name, Engine.AssetLoader.LoadedAssets.Select(x => x.Name));
        }

        /// <summary>
        /// Test whether shader fallback works.
        /// </summary>
        [Fact]
        public void ShaderFallback()
        {
            ShaderAsset shader = null;

            // Create scene for this test.
            TestScene extScene = new TestScene
            {
                // Load a shader. Also tests whether loading of shaders within another thread works.
                ExtLoad = () => { shader = Engine.AssetLoader.Get<ShaderAsset>("Shaders/BrokenShaderWithFallback.xml"); },
                // Until will unload the shaders.
                ExtUnload = () => { Engine.AssetLoader.Destroy(shader.Name); },
                // Draw all textures in a grid.
                ExtDraw = () =>
                {
                    Engine.Renderer.SetShader(shader.Shader);

                    // Render a blank square.
                    Engine.Renderer.Render(new Vector3(10, 10, 0), new Vector2(10, 10), Color.White);

                    // Reset to default shader.
                    Engine.Renderer.SetShader();
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // The shader should've loaded its fallback.
            Assert.NotNull(shader.Shader);
            Assert.True(shader.IsFallback);

            // Check if what is currently on screen is what is expected.
            Assert.Equal("PKlb8X5gkVtCWie3LIKpcYzEc5nNjr2Xrz/gCqs54yA=", Helpers.TakeScreenshot());

            // Cleanup.
            Helpers.UnloadScene();

            // Ensure the shaders are unloaded.
            Assert.DoesNotContain(shader.Name, Engine.AssetLoader.LoadedAssets.Select(x => x.Name));
        }
    }
}