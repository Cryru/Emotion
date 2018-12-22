// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Linq;
using System.Numerics;
using Emotion.Engine;
using Emotion.Graphics;
using Emotion.Graphics.Batching;
using Emotion.Graphics.Text;
using Emotion.Primitives;
using Emotion.Tests.Interoperability;
using Emotion.Tests.Layers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Emotion.Tests.Tests
{
    /// <summary>
    /// Tests connected with texture drawing.
    /// </summary>
    [TestClass]
    public class TextureDrawing
    {
        /// <summary>
        /// Test whether loading and drawing of textures works.
        /// </summary>
        [TestMethod]
        public void TextureLoadingAndDrawing()
        {
            // Only the first frame of the gif will be rendered.
            string[] textures =
            {
                "Textures/standardPng.png", "Textures/standardJpg.jpg", "Textures/standardGif.gif",
                "Textures/standardTif.tif", "Textures/16bmp.bmp", "Textures/24bitbmp.bmp", "Textures/256bmp.bmp"
            };

            // Get the host.
            TestHost host = TestInit.TestingHost;

            // Create layer for this test.
            ExternalLayer extLayer = new ExternalLayer
            {
                // This will test loading testing in another thread.
                ExtLoad = () =>
                {
                    foreach (string t in textures)
                    {
                        Context.AssetLoader.Get<Texture>(t);
                    }
                },
                // This will test unloading of textures.
                ExtUnload = () =>
                {
                    foreach (string t in textures)
                    {
                        Context.AssetLoader.Destroy(t);
                    }
                },
                // Draw all textures in a grid.
                ExtDraw = () =>
                {
                    for (int i = 0; i < textures.Length; i++)
                    {
                        int xLoc = 10 + i * 105;
                        Context.Renderer.Render(new Vector3(xLoc, 10, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>(textures[i]));
                    }
                }
            };

            // Add layer.
            Helpers.LoadLayer(extLayer, "texture test layer");

            // Check if what is currently on screen is what is expected.
            Assert.AreEqual("lsDdE6FX8qi9qc/Q/hxE9XbduqIBFK9QICiLzjoLxCQ=", host.TakeScreenshot().Hash());

            // Cleanup layer.
            Helpers.UnloadLayer(extLayer);

            // Ensure no layers are left loaded.
            Assert.AreEqual(0, Context.LayerManager.LoadedLayers.Length);

            // Ensure the textures are unloaded.
            Assert.AreEqual(textures.Length, textures.Except(Context.AssetLoader.LoadedAssets.Select(x => x.Name)).Count());
        }

        /// <summary>
        /// Test whether drawing of textures with alpha and tinting works as expected.
        /// </summary>
        [TestMethod]
        public void AlphaTextureDrawing()
        {
            // Get the host.
            TestHost host = TestInit.TestingHost;

            // Create layer for this test.
            ExternalLayer extLayer = new ExternalLayer
            {
                // This will test loading testing in another thread.
                ExtLoad = () =>
                {
                    Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"); // Has alpha built in.
                    Context.AssetLoader.Get<Texture>("Textures/standardPng.png"); // Will be drawn with tinted alpha.
                    Context.AssetLoader.Get<Texture>("Textures/standardGif.gif"); // Format doesn't support alpha, will be drawn with tinted alpha.
                },
                // This will test unloading of textures.
                ExtUnload = () =>
                {
                    Context.AssetLoader.Destroy("Textures/logoAlpha.png");
                    Context.AssetLoader.Destroy("Textures/standardPng.png");
                    Context.AssetLoader.Destroy("Textures/standardGif.gif");
                },
                // Draw the textures.
                ExtDraw = () =>
                {
                    Context.Renderer.Render(new Vector3(10, 10, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(105, 10, 0), new Vector2(100, 100), new Color(Color.White, 125), Context.AssetLoader.Get<Texture>("Textures/standardPng.png"));
                    Context.Renderer.Render(new Vector3(210, 10, 0), new Vector2(100, 100), new Color(Color.White, 125), Context.AssetLoader.Get<Texture>("Textures/standardGif.gif"));
                }
            };

            // Add layer.
            Helpers.LoadLayer(extLayer, "texture alpha test layer");

            // Check if what is currently on screen is what is expected.
            Assert.AreEqual("MtlaEdCKLT5205oeCNAJP2NYvKbU/iByuf8CbE1jEAw=", host.TakeScreenshot().Hash());

            // Cleanup layer.
            Helpers.UnloadLayer(extLayer);

            // Ensure no layers are left loaded.
            Assert.AreEqual(0, Context.LayerManager.LoadedLayers.Length);

            // Ensure the textures are unloaded.
            Assert.AreEqual(null, Context.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/logoAlpha.png"));
            Assert.AreEqual(null, Context.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/standardPng.png"));
            Assert.AreEqual(null, Context.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/standardGif.gif"));
        }

        /// <summary>
        /// Tests drawing with different Z coordinates and how the result overlaps and is handled.
        /// </summary>
        [TestMethod]
        public void TextureDepthTest()
        {
            // Get the host.
            TestHost host = TestInit.TestingHost;

            // Reference a map buffer to test drawing with that as well.
            QuadMapBuffer buffer = null;

            // Create layer for this test.
            ExternalLayer extLayer = new ExternalLayer
            {
                // Load the textures.
                ExtLoad = () =>
                {
                    Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png");

                    // Also tests mapping and initializing of map buffers in another thread.
                    buffer = new QuadMapBuffer(100);
                    buffer.MapNextQuad(new Vector3(Context.Settings.RenderSettings.Width / 2 - 100, 150, 0), new Vector2(20, 20), Color.White);
                    buffer.MapNextQuad(new Vector3(Context.Settings.RenderSettings.Width / 2 - 100, 180, 0), new Vector2(20, 20), Color.White);
                    buffer.MapNextQuad(new Vector3(Context.Settings.RenderSettings.Width / 2 - 100, 210, 0), new Vector2(20, 20), Color.White);
                    buffer.MapNextQuad(new Vector3(Context.Settings.RenderSettings.Width / 2 - 100, 240, 0), new Vector2(20, 20), Color.White);
                    buffer.MapNextQuad(new Vector3(Context.Settings.RenderSettings.Width / 2 - 100, 270, 0), new Vector2(20, 20), Color.White);
                    buffer.MapNextQuad(new Vector3(Context.Settings.RenderSettings.Width / 2 - 100, 300, 0), new Vector2(20, 20), Color.White);
                    buffer.MapNextQuad(new Vector3(Context.Settings.RenderSettings.Width / 2 - 100, 330, 0), new Vector2(20, 20), Color.White);
                },
                // Unload the texture.
                ExtUnload = () =>
                {
                    Context.AssetLoader.Destroy("Textures/logoAlpha.png");

                    // Test unloading of a map buffer.
                    buffer.Delete();
                },
                // Draw textures.
                ExtDraw = () =>
                {
                    int maxX = 5 * 49;
                    int maxY = 5 * 49;

                    // Set background so we can see invalid alpha.
                    Context.Renderer.Render(new Vector3(0, 0, -1), new Vector2(Context.Settings.RenderSettings.Width, Context.Settings.RenderSettings.Height), Color.CornflowerBlue);

                    // Draw normally.
                    for (int i = 0; i < 50; i++)
                    {
                        Context.Renderer.Render(new Vector3(5 * i, 5 * i, i), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    }

                    for (int i = 0; i < 50; i++)
                    {
                        Context.Renderer.Render(new Vector3(5 * i, maxY - 5 * i, i), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    }

                    // Queue draw.
                    for (int i = 0; i < 50; i++)
                    {
                        Context.Renderer.RenderQueue(new Vector3(maxX + 5 * i, maxY + 5 * i, i), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    }

                    for (int i = 0; i < 50; i++)
                    {
                        Context.Renderer.RenderQueue(new Vector3(maxX + 5 * i, maxY + maxY - 5 * i, i + 49), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    }

                    // Draw line 0-1/1-0 with queuing.
                    Context.Renderer.RenderQueue(new Vector3(Context.Settings.RenderSettings.Width, 0, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.RenderQueue(new Vector3(Context.Settings.RenderSettings.Width - 50, 0, 1), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.RenderQueue(new Vector3(Context.Settings.RenderSettings.Width - 100, 0, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.RenderQueue(new Vector3(Context.Settings.RenderSettings.Width - 150, 0, 1), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.RenderQueue(new Vector3(Context.Settings.RenderSettings.Width - 200, 0, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.RenderQueue(new Vector3(Context.Settings.RenderSettings.Width - 250, 0, 1), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.RenderQueue(new Vector3(Context.Settings.RenderSettings.Width - 300, 0, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));

                    Context.Renderer.RenderQueue(new Vector3(Context.Settings.RenderSettings.Width, 100, 1), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.RenderQueue(new Vector3(Context.Settings.RenderSettings.Width - 50, 100, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.RenderQueue(new Vector3(Context.Settings.RenderSettings.Width - 100, 100, 1), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.RenderQueue(new Vector3(Context.Settings.RenderSettings.Width - 150, 100, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.RenderQueue(new Vector3(Context.Settings.RenderSettings.Width - 200, 100, 1), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.RenderQueue(new Vector3(Context.Settings.RenderSettings.Width - 250, 100, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.RenderQueue(new Vector3(Context.Settings.RenderSettings.Width - 300, 100, 1), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));

                    // Render line 0-1/1-0 without queing.
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width, 200, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 50, 200, 1), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 100, 200, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 150, 200, 1), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 200, 200, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 250, 200, 1), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 300, 200, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));

                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width, 300, 1), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 50, 300, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 100, 300, 1), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 150, 300, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 200, 300, 1), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 250, 300, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 300, 300, 1), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));

                    // Draw a map buffer.
                    Context.Renderer.Render(buffer);

                    // Render text.
                    Context.Renderer.RenderString(Context.AssetLoader.Get<Font>("debugFont.otf"), 15, "This is test text", new Vector3(Context.Settings.RenderSettings.Width / 2 - 100, 0, 1), Color.Black);
                    Context.Renderer.RenderString(Context.AssetLoader.Get<Font>("debugFont.otf"), 15, "This is test text", new Vector3(Context.Settings.RenderSettings.Width / 2 - 100, 10, 2), Color.Black);
                    Context.Renderer.RenderString(Context.AssetLoader.Get<Font>("debugFont.otf"), 15, "This is test text", new Vector3(Context.Settings.RenderSettings.Width / 2 - 100, 20, 1), Color.Black);
                }
            };

            // Add layer.
            Helpers.LoadLayer(extLayer, "texture depth test layer");

            // Check if what is currently on screen is what is expected.
            // This render is not 100% correct though, the hearts on the right for instance shouldn't be smooth as it is an alternating 0-1 chain.
            // This has to do with the DepthFunc being set to GL_Always.
            Assert.AreEqual("cZswFyUuG7sNprOMV95w1JRTnOIgyXOqICyzDHlKk5o=", host.TakeScreenshot().Hash());

            // Cleanup layer.
            Helpers.UnloadLayer(extLayer);

            // Ensure no layers are left loaded.
            Assert.AreEqual(0, Context.LayerManager.LoadedLayers.Length);

            // Ensure the textures are unloaded.
            Assert.AreEqual(null, Context.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/logoAlpha.png"));
        }
    }
}