// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Engine;
using Emotion.Graphics;
using Emotion.Graphics.Base;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Text;
using Emotion.Primitives;
using Emotion.Tests.Interoperability;
using Emotion.Tests.Scenes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Emotion.Tests.Tests
{
    /// <summary>
    /// Tests connected with drawing.
    /// </summary>
    [TestClass]
    public class Drawing
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

            // Create scene for this test.
            ExternalScene extScene = new ExternalScene
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

            // Load scene.
            Helpers.LoadScene(extScene);

            // Check if what is currently on screen is what is expected.
            Assert.AreEqual("lsDdE6FX8qi9qc/Q/hxE9XbduqIBFK9QICiLzjoLxCQ=", host.TakeScreenshot().Hash());

            // Cleanup.
            Helpers.UnloadScene();

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

            // Create scene for this test.
            ExternalScene extScene = new ExternalScene
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

            // Load scene.
            Helpers.LoadScene(extScene);

            // Check if what is currently on screen is what is expected.
            Assert.AreEqual("/0r3NzR0HB+t6UfGxLiIWhAdghVnnlb0F8hTbmFutQU=", host.TakeScreenshot().Hash());

            // Cleanup.
            Helpers.UnloadScene();

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
            Task.Delay(2000).Wait();
            // Get the host.
            TestHost host = TestInit.TestingHost;

            // Reference a map buffer to test drawing with that as well.
            StreamBuffer buffer = null;

            // Create scene for this test.
            ExternalScene extScene = new ExternalScene
            {
                // Load the textures.
                ExtLoad = () =>
                {
                    Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png");

                    // Also tests mapping and initializing of map buffers in another thread.
                    buffer = GraphicsManager.CreateQuadMapBuffer(100);
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
                        Context.Renderer.Render(new Vector3(maxX + 5 * i, maxY + 5 * i, i), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    }

                    for (int i = 0; i < 50; i++)
                    {
                        Context.Renderer.Render(new Vector3(maxX + 5 * i, maxY + maxY - 5 * i, i + 49), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    }

                    // Draw line 0-1/1-0 with queuing.
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width, 0, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 50, 0, 1), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 100, 0, 0), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 150, 0, 1), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 200, 0, 0), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 250, 0, 1), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 300, 0, 0), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));

                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width, 100, 1), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 50, 100, 0), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 100, 100, 1), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 150, 100, 0), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 200, 100, 1), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 250, 100, 0), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 300, 100, 1), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));

                    // Render line 0-1/1-0 without queing.
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width, 200, 0), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 50, 200, 1), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 100, 200, 0), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 150, 200, 1), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 200, 200, 0), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 250, 200, 1), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 300, 200, 0), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));

                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width, 300, 1), new Vector2(100, 100), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 50, 300, 0), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 100, 300, 1), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 150, 300, 0), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 200, 300, 1), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 250, 300, 0), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width - 300, 300, 1), new Vector2(100, 100), Color.White,
                        Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));

                    // Draw a map buffer.
                    Context.Renderer.Render(buffer);

                    // Render text.
                    Context.Renderer.RenderString(Context.AssetLoader.Get<Font>("debugFont.otf"), 20, "This is test text", new Vector3(Context.Settings.RenderSettings.Width / 2 - 100, 0, 1),
                        Color.Red);
                    Context.Renderer.RenderString(Context.AssetLoader.Get<Font>("debugFont.otf"), 20, "This is test text", new Vector3(Context.Settings.RenderSettings.Width / 2 - 100, 10, 2),
                        Color.Green);
                    Context.Renderer.RenderString(Context.AssetLoader.Get<Font>("debugFont.otf"), 20, "This is test text", new Vector3(Context.Settings.RenderSettings.Width / 2 - 100, 20, 1),
                        Color.Blue);
                    Context.Renderer.Render(new Vector3(Context.Settings.RenderSettings.Width / 2 - 100, 0, 0), new Vector2(200, 100), Color.Black);
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Check if what is currently on screen is what is expected.
            // This render is not 100% correct though, the hearts on the right for instance shouldn't be smooth as it is an alternating 0-1 chain.
            // This has to do with the DepthFunc being set to GL_Always.
            Assert.AreEqual("J0m0+IMHE2dtLnk1OLyKCS/YDqka8q79YmJ66P0Fzjw=", host.TakeScreenshot().Hash());

            // Cleanup.
            Helpers.UnloadScene();

            // Ensure the textures are unloaded.
            Assert.AreEqual(null, Context.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/logoAlpha.png"));
        }

        /// <summary>
        /// Tests map buffer functions and rendering.
        /// </summary>
        [TestMethod]
        public void MapBufferTest()
        {
            // Get the host.
            TestHost host = TestInit.TestingHost;

            // Shader to test shader drawing.
            ShaderProgram testShader = new ShaderProgram(null, @"#version 300 es

#ifdef GL_ES
precision highp float;
#endif

uniform sampler2D textures[16];

// Comes in from the vertex shader.
in vec2 UV;
in vec4 vertColor;
in float Tid;

out vec4 fragColor;

void main() {
    vec4 temp;

    // Check if a texture is in use.
    if (Tid >= 0.0)
    {
        // Sample for the texture's color at the specified vertex UV and multiply it by the tint.
        temp = texture(textures[int(Tid)], UV) * vertColor;
    } else {
        // If no texture then just use the color.
        temp = vertColor;
    }

    fragColor = vec4(temp.y, temp.x, 0, temp.w);

    if (fragColor.a < 0.01) discard;
}");

            // Reference map buffers to test with.
            StreamBuffer quadBuffer = null;
            StreamBuffer overflowVerts = null;
            StreamBuffer overflowTextures = null;
            StreamBuffer colorBarfBuffer = null;

            bool exceptionRaised = false;

            // Create scene for this test.
            ExternalScene extScene = new ExternalScene
            {
                // Load the textures.
                ExtLoad = () =>
                {
                    // Init quad buffer.
                    quadBuffer = GraphicsManager.CreateQuadMapBuffer(20);
                    quadBuffer.MapNextQuad(new Vector3(5, 10, 0), new Vector2(20, 20), Color.White);
                    quadBuffer.MapNextQuad(new Vector3(5, 40, 0), new Vector2(20, 20), Color.White);
                    quadBuffer.MapNextQuad(new Vector3(5, 70, 0), new Vector2(20, 20), Color.White);
                    quadBuffer.MapNextQuad(new Vector3(5, 100, 0), new Vector2(20, 20), Color.White);
                    quadBuffer.MapNextQuad(new Vector3(5, 130, 0), new Vector2(20, 20), Color.White);
                    quadBuffer.MapNextQuad(new Vector3(5, 160, 0), new Vector2(20, 20), Color.White);
                    quadBuffer.MapNextQuad(new Vector3(5, 190, 0), new Vector2(20, 20), Color.White);

                    // Init overflow buffer.
                    // The size is smaller than what we are mapping, the expected behavior is not to map the third one.
                    overflowVerts = GraphicsManager.CreateQuadMapBuffer(2);
                    overflowVerts.MapNextQuad(new Vector3(5, 10, 0), new Vector2(20, 20), Color.White);
                    overflowVerts.MapNextQuad(new Vector3(5, 40, 0), new Vector2(20, 20), Color.White);
                    overflowVerts.MapNextQuad(new Vector3(5, 70, 0), new Vector2(20, 20), Color.White);

                    // Init a buffer which will overflow the texture limit.
                    Context.Flags.RenderFlags.TextureArrayLimit = 2;
                    overflowTextures = GraphicsManager.CreateQuadMapBuffer(30);
                    overflowTextures.MapNextQuad(new Vector3(5, 10, 0), new Vector2(20, 20), Color.White, Context.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    overflowTextures.MapNextQuad(new Vector3(5, 40, 0), new Vector2(20, 20), Color.White, Context.AssetLoader.Get<Texture>("Textures/standardPng.png"));

                    try
                    {
                        overflowTextures.MapNextQuad(new Vector3(5, 70, 0), new Vector2(20, 20), Color.White, Context.AssetLoader.Get<Texture>("Textures/standardGif.gif"));
                    }
                    catch (Exception)
                    {
                        exceptionRaised = true;
                    }

                    // An exception should've been raised.
                    Assert.IsTrue(exceptionRaised);

                    // Map color barf.
                    colorBarfBuffer = GraphicsManager.CreateQuadMapBuffer(100);
                    int x = 0;
                    int y = 0;
                    const int size = 5;
                    for (int i = 0; i < 100; i++)
                    {
                        // Map quad.
                        colorBarfBuffer.MapNextQuad(new Vector3(x * size, y * size, 1), new Vector2(size, size), new Color(i, 255 - i, 125 + i));

                        // Grid logic.
                        x++;
                        if (x * size < 25)
                            continue;
                        x = 0;
                        y++;
                    }
                },
                // Unload the texture.
                ExtUnload = () =>
                {
                    // Unloads buffers.
                    quadBuffer.Delete();
                    overflowVerts.Delete();
                    overflowTextures.Delete();
                    colorBarfBuffer.Delete();

                    // Unload textures.
                    Context.AssetLoader.Destroy("Textures/logoAlpha.png");
                    Context.AssetLoader.Destroy("Textures/standardPng.png");
                    Context.AssetLoader.Destroy("Textures/standardGif.gif");
                },
                // Draw textures.
                ExtDraw = () =>
                {
                    // Draw a map buffer.
                    Context.Renderer.Render(quadBuffer);

                    // Now draw it with a shader and a matrix.
                    Context.Renderer.SetShader(testShader);
                    Context.Renderer.PushToModelMatrix(Matrix4x4.CreateTranslation(25, 0, 0));

                    Context.Renderer.Render(quadBuffer);

                    Context.Renderer.PopModelMatrix();
                    Context.Renderer.SetShader();

                    // Draw overflow.
                    Context.Renderer.PushToModelMatrix(Matrix4x4.CreateTranslation(50, 0, 0));
                    Context.Renderer.Render(overflowVerts);
                    Context.Renderer.PopModelMatrix();

                    // Draw texture overflow.
                    Context.Renderer.PushToModelMatrix(Matrix4x4.CreateTranslation(75, 0, 0));
                    Context.Renderer.Render(overflowTextures);
                    Context.Renderer.PopModelMatrix();

                    // Draw color barf.
                    Context.Renderer.PushToModelMatrix(Matrix4x4.CreateTranslation(100, 0, 0));
                    Context.Renderer.Render(colorBarfBuffer);
                    Context.Renderer.PopModelMatrix();
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Check if what is currently on screen is what is expected.
            Assert.AreEqual("SfWiHfUEc5BBSpivGu4jLUCwxO6GsXEGD+mr4IC1CFs=", host.TakeScreenshot().Hash());

            // Remap the first square and the tenth in the color barf buffer to test arbitrary remapping.
            colorBarfBuffer.MapQuadAt(0, new Vector3(0, 0, 1), new Vector2(5, 5), new Color(255, 255, 255));
            colorBarfBuffer.MapQuadAt(10, new Vector3(250, 0, 1), new Vector2(5, 5), new Color(255, 255, 255));

            // Run a cycle (two for double buffering) to draw the changed map.
            host.RunCycle();
            host.RunCycle();
            Assert.AreEqual("R/Avj2tjahUl4v4ans2vk5feOWUM+QJ2ec41i9Go6sI=", host.TakeScreenshot().Hash());

            // Set render range, and test rendering with that.
            colorBarfBuffer.SetRenderRange(0, 10);
            host.RunCycle();
            host.RunCycle();
            Assert.AreEqual("Ew/b2KiGeLLzJuUvKW674tk2wvwy3UxYHjeBgMednqk=", host.TakeScreenshot().Hash());

            // Cleanup.
            Helpers.UnloadScene();

            // Ensure the textures are unloaded.
            Assert.AreEqual(null, Context.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/logoAlpha.png"));
            Assert.AreEqual(null, Context.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/standardPng.png"));
            Assert.AreEqual(null, Context.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/standardGif.gif"));
        }
    }
}