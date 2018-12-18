// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Linq;
using System.Numerics;
using Emotion.Engine;
using Emotion.Graphics;
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
    }
}