// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Emotion.Engine;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Tests.Interoperability;
using Emotion.Tests.Scenes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Emotion.Tests.Tests
{
    /// <summary>
    /// Tests related to testing the shader functionality of Emotion.
    /// </summary>
    [TestClass]
    public class Shaders
    {
        /// <summary>
        /// Test whether loading of shaders and drawing with them works.
        /// Also tests loading of shaders with vert and without frag.
        /// </summary>
        [TestMethod]
        public void ShaderLoadAndDraw()
        {
            List<ShaderAsset> shaders = new List<ShaderAsset>();

            // Get the host.
            TestHost host = TestInit.TestingHost;

            // Which phase of the test is happening. Done to split the draw function.
            int shaderTest = 0;

            // Create scene for this test.
            ExternalScene extScene = new ExternalScene
            {
                // Load a shader. Also tests whether loading of shaders within another thread works.
                ExtLoad = () =>
                {
                    shaders.Add(Context.AssetLoader.Get<ShaderAsset>("Shaders/TestShader.xml"));
                    shaders.Add(Context.AssetLoader.Get<ShaderAsset>("Shaders/TestShaderFragOnly.xml"));
                    shaders.Add(Context.AssetLoader.Get<ShaderAsset>("Shaders/TestShaderVertOnly.xml"));
                },
                // Until will unload the shaders.
                ExtUnload = () =>
                {
                    foreach (ShaderAsset s in shaders)
                    {
                        Context.AssetLoader.Destroy(s.Name);
                    }
                },
                // Draw all textures in a grid.
                ExtDraw = () =>
                {
                    // Set shader.
                    if (shaderTest != 0)
                    {
                        Context.Renderer.SetShader(shaders[shaderTest - 1].Shader);
                    }

                    // Render a blank square.
                    Context.Renderer.Render(new Vector3(10, 10, 0), new Vector2(10, 10), Color.White);

                    // Reset to default shader.
                    Context.Renderer.SetShader();
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Ensure all shaders are loaded.
            foreach (ShaderAsset s in shaders)
            {
                Assert.AreNotEqual(null, s.Shader);
                Assert.IsFalse(s.IsFallback);
            }
            host.RunCycle(16);

            // Check if what is currently on screen is what is expected.
            string o = host.TakeScreenshot().Hash();
            Assert.AreEqual("ARc+sjeja/e/8OH0dMxFgw6BdoVpZTOshst5wRTK6XA=", host.TakeScreenshot().Hash());

            // Change to phase 1. This is drawing with the test shader.
            shaderTest = 1;
            host.RunCycle(16);
            host.RunCycle(16);
            Assert.AreEqual("716eI7w9eupNhgRCuaB3EfEYSw+xhU08tGegUUbYv88=", host.TakeScreenshot().Hash());

            // Change to phase 2. This is drawing with the vert missing shader.
            shaderTest = 2;
            host.RunCycle(16);
            host.RunCycle(16);
            Assert.AreEqual("HJUXXghymwkV4iZ3wwIOBE4luD5zYpF/8AIXTGuSOy4=", host.TakeScreenshot().Hash());

            // Change to phase 3. This is drawing with the frag missing shader.
            shaderTest = 3;
            host.RunCycle(16);
            host.RunCycle(16);
            Assert.AreEqual("ARc+sjeja/e/8OH0dMxFgw6BdoVpZTOshst5wRTK6XA=", host.TakeScreenshot().Hash());

            // Cleanup.
            Helpers.UnloadScene();

            // Ensure the shaders are unloaded.
            Assert.AreEqual(shaders.Count, shaders.Select(x => x.Name).Except(Context.AssetLoader.LoadedAssets.Select(x => x.Name)).Count());
        }

        /// <summary>
        /// Test whether loading of broken shaders behaves as expected.
        /// </summary>
        [TestMethod]
        public void ShaderBrokenLoad()
        {
            ShaderAsset shader = null;

            // Get the host.
            TestHost host = TestInit.TestingHost;

            // Create scene for this test.
            ExternalScene extScene = new ExternalScene
            {
                // Load a shader. Also tests whether loading of shaders within another thread works.
                ExtLoad = () => { shader = Context.AssetLoader.Get<ShaderAsset>("Shaders/BrokenShader.xml"); },
                // Until will unload the shaders.
                ExtUnload = () =>
                {
                    Context.AssetLoader.Destroy(shader.Name);
                },
                // Draw all textures in a grid.
                ExtDraw = () =>
                {
                    Context.Renderer.SetShader(shader.Shader);

                    // Render a blank square.
                    Context.Renderer.Render(new Vector3(10, 10, 0), new Vector2(10, 10), Color.White);

                    // Reset to default shader.
                    Context.Renderer.SetShader();
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Ensure all shaders are loaded.
            Assert.AreEqual(null, shader.Shader);
            Assert.IsFalse(shader.IsFallback);
            host.RunCycle(16);

            // Check if what is currently on screen is what is expected.
            Assert.AreEqual("ARc+sjeja/e/8OH0dMxFgw6BdoVpZTOshst5wRTK6XA=", host.TakeScreenshot().Hash());

            // Cleanup.
            Helpers.UnloadScene();

            // Ensure the shaders are unloaded.
            Assert.IsFalse(Context.AssetLoader.LoadedAssets.Select(x => x.Name).Contains(shader.Name));
        }

        /// <summary>
        /// Test whether shader fallback works.
        /// </summary>
        [TestMethod]
        public void ShaderFallback()
        {
            ShaderAsset shader = null;

            // Get the host.
            TestHost host = TestInit.TestingHost;

            // Create scene for this test.
            ExternalScene extScene = new ExternalScene
            {
                // Load a shader. Also tests whether loading of shaders within another thread works.
                ExtLoad = () => { shader = Context.AssetLoader.Get<ShaderAsset>("Shaders/BrokenShaderWithFallback.xml"); },
                // Until will unload the shaders.
                ExtUnload = () =>
                {
                    Context.AssetLoader.Destroy(shader.Name);
                },
                // Draw all textures in a grid.
                ExtDraw = () =>
                {
                    Context.Renderer.SetShader(shader.Shader);

                    // Render a blank square.
                    Context.Renderer.Render(new Vector3(10, 10, 0), new Vector2(10, 10), Color.White);

                    // Reset to default shader.
                    Context.Renderer.SetShader();
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Ensure all shaders are loaded.
            Assert.AreNotEqual(null, shader.Shader);
            Assert.IsTrue(shader.IsFallback);
            host.RunCycle(16);

            // Check if what is currently on screen is what is expected.
            Assert.AreEqual("716eI7w9eupNhgRCuaB3EfEYSw+xhU08tGegUUbYv88=", host.TakeScreenshot().Hash());

            // Cleanup.
            Helpers.UnloadScene();

            // Ensure the shaders are unloaded.
            Assert.IsFalse(Context.AssetLoader.LoadedAssets.Select(x => x.Name).Contains(shader.Name));
        }
    }
}