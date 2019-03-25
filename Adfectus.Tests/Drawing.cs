#region Using

using System.Linq;
using System.Numerics;
using Adfectus.Common;
using Adfectus.Game.Tiled;
using Adfectus.Graphics;
using Adfectus.Graphics.Text;
using Adfectus.Primitives;
using Adfectus.Tests.Scenes;
using Xunit;

#endregion

namespace Adfectus.Tests
{
    /// <summary>
    /// Tests connected with drawing and the Renderer module.
    /// </summary>
    [Collection("main")]
    public class Drawing
    {
        /// <summary>
        /// Test whether loading and drawing of textures works.
        /// </summary>
        [Fact]
        public void TextureLoadingAndDrawing()
        {
            // Only the first frame of the gif will be rendered.
            string[] textures =
            {
                "Textures/standardPng.png", "Textures/standardJpg.jpg", "Textures/standardGif.gif",
                "Textures/standardTif.tif", "Textures/16bmp.bmp", "Textures/24bitbmp.bmp", "Textures/256bmp.bmp"
            };

            // Create scene for this test.
            TestScene extScene = new TestScene
            {
                // This will test loading textures in another thread as well.
                ExtLoad = () =>
                {
                    foreach (string t in textures)
                    {
                        Engine.AssetLoader.Get<Texture>(t);
                    }
                },
                // This will test unloading of textures.
                ExtUnload = () =>
                {
                    foreach (string t in textures)
                    {
                        Engine.AssetLoader.Destroy(t);
                    }
                },
                // Draw all textures in a grid.
                ExtDraw = () =>
                {
                    for (int i = 0; i < textures.Length; i++)
                    {
                        int xLoc = 10 + i * 105;
                        Engine.Renderer.Render(new Vector3(xLoc, 10, 0), new Vector2(100, 100), Color.White, Engine.AssetLoader.Get<Texture>(textures[i]));
                    }
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Check if what is currently on screen is what is expected.
            Assert.Equal("0wj4xyKUwSDvOShUEmdaRhWyP3a6eA1Lzwtesb7rqHA=", Helpers.TakeScreenshot());

            // Cleanup.
            Helpers.UnloadScene();

            // Ensure the textures are unloaded.
            Assert.Equal(textures.Length, textures.Except(Engine.AssetLoader.LoadedAssets.Select(x => x.Name)).Count());
        }

        /// <summary>
        /// Test whether drawing of textures with alpha and tinting works as expected.
        /// </summary>
        [Fact]
        public void AlphaTextureDrawing()
        {
            // Create scene for this test.
            TestScene extScene = new TestScene
            {
                // This will test loading testing in another thread.
                ExtLoad = () =>
                {
                    Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"); // Has alpha built in.
                    Engine.AssetLoader.Get<Texture>("Textures/standardPng.png"); // Will be drawn with tinted alpha.
                    Engine.AssetLoader.Get<Texture>("Textures/standardGif.gif"); // Format doesn't support alpha, will be drawn with tinted alpha.
                },
                // This will test unloading of textures.
                ExtUnload = () =>
                {
                    Engine.AssetLoader.Destroy("Textures/logoAlpha.png");
                    Engine.AssetLoader.Destroy("Textures/standardPng.png");
                    Engine.AssetLoader.Destroy("Textures/standardGif.gif");
                },
                // Draw the textures.
                ExtDraw = () =>
                {
                    Engine.Renderer.Render(new Vector3(10, 10, 0), new Vector2(100, 100), Color.White, Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(105, 10, 0), new Vector2(100, 100), new Color(Color.White, 125), Engine.AssetLoader.Get<Texture>("Textures/standardPng.png"));
                    Engine.Renderer.Render(new Vector3(210, 10, 0), new Vector2(100, 100), new Color(Color.White, 125), Engine.AssetLoader.Get<Texture>("Textures/standardGif.gif"));
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Check if what is currently on screen is what is expected.
            Assert.Equal("8aivucpanUVd+Ji0bDXfgk6H4N50z+MeNwlkzUhQGas=", Helpers.TakeScreenshot());

            // Cleanup.
            Helpers.UnloadScene();

            // Ensure the textures are unloaded.
            Assert.Null(Engine.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/logoAlpha.png"));
            Assert.Null(Engine.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/standardPng.png"));
            Assert.Null(Engine.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/standardGif.gif"));
        }

        /// <summary>
        /// Tests drawing with different Z coordinates and how the result overlaps and is handled.
        /// </summary>
        [Fact]
        public void TextureDepthTest()
        {
            // Reference a map buffer to test drawing with that as well.
            StreamBuffer buffer = null;

            // Create scene for this test.
            TestScene extScene = new TestScene
            {
                // Load the textures.
                ExtLoad = () =>
                {
                    Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png");

                    // Also tests mapping and initializing of map buffers in another thread.
                    buffer = Engine.GraphicsManager.CreateQuadStreamBuffer(100);
                    buffer.MapNextQuad(new Vector3(Engine.GraphicsManager.RenderSize.X / 2 - 100, 150, 0), new Vector2(20, 20), Color.White);
                    buffer.MapNextQuad(new Vector3(Engine.GraphicsManager.RenderSize.X / 2 - 100, 180, 0), new Vector2(20, 20), Color.White);
                    buffer.MapNextQuad(new Vector3(Engine.GraphicsManager.RenderSize.X / 2 - 100, 210, 0), new Vector2(20, 20), Color.White);
                    buffer.MapNextQuad(new Vector3(Engine.GraphicsManager.RenderSize.X / 2 - 100, 240, 0), new Vector2(20, 20), Color.White);
                    buffer.MapNextQuad(new Vector3(Engine.GraphicsManager.RenderSize.X / 2 - 100, 270, 0), new Vector2(20, 20), Color.White);
                    buffer.MapNextQuad(new Vector3(Engine.GraphicsManager.RenderSize.X / 2 - 100, 300, 0), new Vector2(20, 20), Color.White);
                    buffer.MapNextQuad(new Vector3(Engine.GraphicsManager.RenderSize.X / 2 - 100, 330, 0), new Vector2(20, 20), Color.White);
                },
                // Unload the texture.
                ExtUnload = () =>
                {
                    Engine.AssetLoader.Destroy("Textures/logoAlpha.png");

                    // Test unloading of a map buffer.
                    buffer.Delete();
                },
                // Draw textures.
                ExtDraw = () =>
                {
                    const int maxX = 5 * 49;
                    const int maxY = 5 * 49;

                    // Set background so we can see invalid alpha.
                    Engine.Renderer.Render(new Vector3(0, 0, -1), Engine.GraphicsManager.RenderSize, Color.CornflowerBlue);

                    // Draw normally.
                    for (int i = 0; i < 50; i++)
                    {
                        Engine.Renderer.Render(new Vector3(5 * i, 5 * i, i), new Vector2(100, 100), Color.White, Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    }

                    for (int i = 0; i < 50; i++)
                    {
                        Engine.Renderer.Render(new Vector3(5 * i, maxY - 5 * i, i), new Vector2(100, 100), Color.White, Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    }

                    // Queue draw.
                    for (int i = 0; i < 50; i++)
                    {
                        Engine.Renderer.Render(new Vector3(maxX + 5 * i, maxY + 5 * i, i), new Vector2(100, 100), Color.White, Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    }

                    for (int i = 0; i < 50; i++)
                    {
                        Engine.Renderer.Render(new Vector3(maxX + 5 * i, maxY + maxY - 5 * i, i + 49), new Vector2(100, 100), Color.White, Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    }

                    // Draw line 0-1/1-0 with queuing.
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X, 0, 0), new Vector2(100, 100), Color.White, Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 50, 0, 1), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 100, 0, 0), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 150, 0, 1), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 200, 0, 0), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 250, 0, 1), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 300, 0, 0), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));

                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X, 100, 1), new Vector2(100, 100), Color.White, Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 50, 100, 0), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 100, 100, 1), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 150, 100, 0), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 200, 100, 1), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 250, 100, 0), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 300, 100, 1), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));

                    // Render line 0-1/1-0 without queuing.
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X, 200, 0), new Vector2(100, 100), Color.White, Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 50, 200, 1), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 100, 200, 0), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 150, 200, 1), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 200, 200, 0), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 250, 200, 1), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 300, 200, 0), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));

                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X, 300, 1), new Vector2(100, 100), Color.White, Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 50, 300, 0), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 100, 300, 1), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 150, 300, 0), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 200, 300, 1), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 250, 300, 0), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X - 300, 300, 1), new Vector2(100, 100), Color.White,
                        Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));

                    // Draw a map buffer.
                    Engine.Renderer.Render(buffer);

                    // Render text.
                    Engine.Renderer.RenderString(Engine.AssetLoader.Get<Font>("debugFont.otf"), 20, "This is test text", new Vector3(Engine.GraphicsManager.RenderSize.X / 2 - 100, 0, 1),
                        Color.Red);
                    Engine.Renderer.RenderString(Engine.AssetLoader.Get<Font>("debugFont.otf"), 20, "This is test text", new Vector3(Engine.GraphicsManager.RenderSize.X / 2 - 100, 10, 2),
                        Color.Green);
                    Engine.Renderer.RenderString(Engine.AssetLoader.Get<Font>("debugFont.otf"), 20, "This is test text", new Vector3(Engine.GraphicsManager.RenderSize.X / 2 - 100, 20, 1),
                        Color.Blue);
                    Engine.Renderer.Render(new Vector3(Engine.GraphicsManager.RenderSize.X / 2 - 100, 0, 0), new Vector2(200, 100), Color.Black);
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Check if what is currently on screen is what is expected.
            // This render is not 100% correct though, as the text has weird artifacts.
            Assert.Equal("YnuPlWmWruXr2t3NIVrCDj4b+fEVf6/DGH/us3q6TvQ=", Helpers.TakeScreenshot());

            // Cleanup.
            Helpers.UnloadScene();

            // Ensure the textures are unloaded.
            Assert.Null(Engine.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/logoAlpha.png"));
        }

        /// <summary>
        /// Tests stream buffer functions and rendering.
        /// </summary>
        [Fact]
        public void StreamBufferTest()
        {
            // Shader to test shader drawing.
            ShaderProgram testShader = Engine.GraphicsManager.CreateShaderProgram(null, @"#version v

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

            // Create scene for this test.
            TestScene extScene = new TestScene
            {
                // Load the textures.
                ExtLoad = () =>
                {
                    // Init quad buffer.
                    quadBuffer = Engine.GraphicsManager.CreateQuadStreamBuffer(20);
                    quadBuffer.MapNextQuad(new Vector3(5, 10, 0), new Vector2(20, 20), Color.White);
                    quadBuffer.MapNextQuad(new Vector3(5, 40, 0), new Vector2(20, 20), Color.White);
                    quadBuffer.MapNextQuad(new Vector3(5, 70, 0), new Vector2(20, 20), Color.White);
                    quadBuffer.MapNextQuad(new Vector3(5, 100, 0), new Vector2(20, 20), Color.White);
                    quadBuffer.MapNextQuad(new Vector3(5, 130, 0), new Vector2(20, 20), Color.White);
                    quadBuffer.MapNextQuad(new Vector3(5, 160, 0), new Vector2(20, 20), Color.White);
                    quadBuffer.MapNextQuad(new Vector3(5, 190, 0), new Vector2(20, 20), Color.White);

                    // Init overflow buffer.
                    // The size is smaller than what we are mapping, the expected behavior is not to map the third one.
                    overflowVerts = Engine.GraphicsManager.CreateQuadStreamBuffer(2);
                    overflowVerts.MapNextQuad(new Vector3(5, 10, 0), new Vector2(20, 20), Color.White);
                    overflowVerts.MapNextQuad(new Vector3(5, 40, 0), new Vector2(20, 20), Color.White);
                    overflowVerts.MapNextQuad(new Vector3(5, 70, 0), new Vector2(20, 20), Color.White);

                    // Set the texture limit really low.
                    int oldLimit = Engine.Flags.RenderFlags.TextureArrayLimit;
                    Engine.Flags.RenderFlags.TextureArrayLimit = 2;

                    // Init a buffer which will overflow the texture limit.
                    overflowTextures = Engine.GraphicsManager.CreateQuadStreamBuffer(30);
                    overflowTextures.MapNextQuad(new Vector3(5, 10, 0), new Vector2(20, 20), Color.White, Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    overflowTextures.MapNextQuad(new Vector3(5, 40, 0), new Vector2(20, 20), Color.White, Engine.AssetLoader.Get<Texture>("Textures/standardPng.png"));

                    ErrorHandler.SuppressErrors = true;
                    overflowTextures.MapNextQuad(new Vector3(5, 70, 0), new Vector2(20, 20), Color.White, Engine.AssetLoader.Get<Texture>("Textures/standardGif.gif"));
                    ErrorHandler.SuppressErrors = false;

                    // Return original limit.
                    Engine.Flags.RenderFlags.TextureArrayLimit = oldLimit;

                    // Map color barf.
                    colorBarfBuffer = Engine.GraphicsManager.CreateQuadStreamBuffer(100);
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
                    Engine.AssetLoader.Destroy("Textures/logoAlpha.png");
                    Engine.AssetLoader.Destroy("Textures/standardPng.png");
                    Engine.AssetLoader.Destroy("Textures/standardGif.gif");

                    testShader.Delete();
                },
                // Draw textures.
                ExtDraw = () =>
                {
                    // Draw a map buffer.
                    Engine.Renderer.Render(quadBuffer);

                    // Now draw it with a shader and a matrix.
                    Engine.Renderer.SetShader(testShader);
                    Engine.Renderer.PushToModelMatrix(Matrix4x4.CreateTranslation(25, 0, 0));

                    Engine.Renderer.Render(quadBuffer);

                    Engine.Renderer.PopModelMatrix();
                    Engine.Renderer.SetShader();

                    // Draw overflow.
                    Engine.Renderer.PushToModelMatrix(Matrix4x4.CreateTranslation(50, 0, 0));
                    Engine.Renderer.Render(overflowVerts);
                    Engine.Renderer.PopModelMatrix();

                    // Draw texture overflow.
                    Engine.Renderer.PushToModelMatrix(Matrix4x4.CreateTranslation(75, 0, 0));
                    Engine.Renderer.Render(overflowTextures);
                    Engine.Renderer.PopModelMatrix();

                    // Draw color barf.
                    Engine.Renderer.PushToModelMatrix(Matrix4x4.CreateTranslation(100, 0, 0));
                    Engine.Renderer.Render(colorBarfBuffer);
                    Engine.Renderer.PopModelMatrix();
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);
            // Check if what is currently on screen is what is expected.
            Assert.Equal("eJmRj5eeyngfsGEkmMJDYBVqK9pzZpOCK6GNctWpVUg=", Helpers.TakeScreenshot());

            // Remap the first square and the tenth in the color barf buffer to test arbitrary remapping.
            colorBarfBuffer.MapQuadAt(0, new Vector3(0, 0, 1), new Vector2(5, 5), new Color(255, 255, 255));
            colorBarfBuffer.MapQuadAt(10, new Vector3(250, 0, 1), new Vector2(5, 5), new Color(255, 255, 255));

            // Run a cycle to draw the changed buffer.
            extScene.WaitFrames(2).Wait();
            Assert.Equal("Znb581mqFE2GUdCaNvlS08jyd7+mUkYQrGMIT9GKPV4=", Helpers.TakeScreenshot());

            // Set render range, and test rendering with that.
            colorBarfBuffer.SetRenderRange(0, 10);
            extScene.WaitFrames(2).Wait();
            Assert.Equal("7iBfVG9OuqOLXFAoMx0u1O6PtBOfPl4m5RVD8kbheBQ=", Helpers.TakeScreenshot());

            // Cleanup.
            Helpers.UnloadScene();

            // Ensure the textures are unloaded.
            Assert.Null(Engine.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/logoAlpha.png"));
            Assert.Null(Engine.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/standardPng.png"));
            Assert.Null(Engine.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/standardGif.gif"));
        }

        
        /// <summary>
        /// Tests stream buffer range drawing.
        /// </summary>
        [Fact]
        public void StreamBufferRangesTest()
        {
            // Reference map buffers to test with.
            StreamBuffer buffer = null;

            // Create scene for this test.
            TestScene extScene = new TestScene
            {
                // Load the textures.
                ExtLoad = () =>
                {
                    // Init quad buffer.
                    buffer = Engine.GraphicsManager.CreateQuadStreamBuffer(20);
                    buffer.MapNextQuad(new Vector3(5, 10, 0), new Vector2(20, 20), Color.Red);
                    buffer.MapNextQuad(new Vector3(5, 40, 0), new Vector2(20, 20), Color.Yellow);
                    buffer.MapNextQuad(new Vector3(5, 70, 0), new Vector2(20, 20), Color.Green);
                    buffer.MapNextQuad(new Vector3(5, 100, 0), new Vector2(20, 20), Color.Blue);
                    buffer.MapNextQuad(new Vector3(5, 130, 0), new Vector2(20, 20), Color.White, Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png"));
                    buffer.MapNextQuad(new Vector3(5, 160, 0), new Vector2(20, 20), Color.White, Engine.AssetLoader.Get<Texture>("Textures/standardPng.png"));
                    buffer.MapNextQuad(new Vector3(5, 190, 0), new Vector2(20, 20), Color.White, Engine.AssetLoader.Get<Texture>("Textures/standardGif.gif"));
                },
                // Unload the texture.
                ExtUnload = () =>
                {
                    // Unloads buffers.
                    buffer.Delete();

                    // Unload textures.
                    Engine.AssetLoader.Destroy("Textures/logoAlpha.png");
                    Engine.AssetLoader.Destroy("Textures/standardPng.png");
                    Engine.AssetLoader.Destroy("Textures/standardGif.gif");
                },
                // Draw textures.
                ExtDraw = () =>
                {
                    // Draw the buffer.
                    Engine.Renderer.Render(buffer);
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            Assert.Equal("/wWfA8rwIzhspD7e4LamEx3vWU85OP6jSEDMJpHXVZ8=", Helpers.TakeScreenshot());

            buffer.SetRenderRange(0, 2);
            extScene.WaitFrames(2).Wait();
            Assert.Equal("T2Z3l1pCmaMw8HKWkR3VyY6lxXJY6L44qyi+wOGp5M8=", Helpers.TakeScreenshot());

            buffer.SetRenderRange(2, 4);
            extScene.WaitFrames(2).Wait();
            Assert.Equal("o+xAalQAizKbGxcdTXktBJgcHQMgnJ1h/43V7r82uLc=", Helpers.TakeScreenshot());

            buffer.SetRenderRange(4);
            extScene.WaitFrames(2).Wait();
            Assert.Equal("0lBVAPJSqH9CXVpvqjJoo4Q+N9ZE85o2cQcQvnfXosc=", Helpers.TakeScreenshot());

            // Cleanup.
            Helpers.UnloadScene();

            // Ensure the textures are unloaded.
            Assert.Null(Engine.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/logoAlpha.png"));
            Assert.Null(Engine.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/standardPng.png"));
            Assert.Null(Engine.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Textures/standardGif.gif"));
        }

        /// <summary>
        /// Test whether the scaling of the window compared to the render resolution works as excepted, and whether UV sampling is
        /// correct on certain scaled when rending a tile map with borders.
        /// This test exists and verifies whether a specific issue was fixed and has not appeared again.
        /// </summary>
        [Fact]
        public void WeirdTileMapScalingTest()
        {
            Map tileMap = null;

            // Create scene for this test.
            TestScene extScene = new TestScene
            {
                // Create a tile map with a tileset which has 1px white borders on all sides of each tile.
                ExtLoad = () =>
                {
                    // Change the resolution to one in which the scaling issue appears.
                    Engine.Host.Size = new Vector2(1008, 594);

                    tileMap = new Map(Vector3.Zero, Vector2.Zero, Engine.AssetLoader, "Tilemap/DeepForest.tmx", "Tilemap/") {Size = new Vector2(600, 400)};
                },
                // Unload the map.
                ExtUnload = () =>
                {
                    // Restore the resolution.
                    Engine.Host.Size = new Vector2(960, 540);

                    tileMap.Reset("", "");
                },
                // Draw the map.
                ExtDraw = () => { Engine.Renderer.Render(tileMap); }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Check if what is currently on screen is what is expected.
            Assert.Equal("MZqlv1R57P1ofwNJzE4XOmyO2Bp2ZLuWaYwzgAgcBcY=", Helpers.TakeScreenshot());

            // Cleanup.
            Helpers.UnloadScene();

            // Ensure the tile map was unloaded.
            Assert.Null(Engine.AssetLoader.LoadedAssets.FirstOrDefault(x => x.Name == "Tilemap/forest.png"));
        }

        /// <summary>
        /// Test whether drawing of arbitrary vertices works.
        /// </summary>
        [Fact]
        public void DrawVertices()
        {
            // Create scene for this test.
            TestScene extScene = new TestScene
            {
                // Draw arbitrary vertices.
                ExtDraw = () =>
                {
                    Vector3[] vertices =
                    {
                        new Vector3(0, 0, 0),
                        new Vector3(0, 10, 0),
                        new Vector3(10, 10, 0),
                        new Vector3(10, 0, 0)
                    };
                    Color[] colors =
                    {
                        Color.Red,
                        Color.Green,
                        Color.Blue,
                        Color.Yellow
                    };

                    Engine.Renderer.RenderVertices(vertices, colors);

                    Engine.Renderer.PushToModelMatrix(Matrix4x4.CreateTranslation(100, 100, 0));
                    Engine.Renderer.RenderVertices(vertices, Color.White);
                    Engine.Renderer.PopModelMatrix();
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Check if what is currently on screen is what is expected.
            Assert.Equal("l5ymQ0MWYBzsM7iFYqKsiKMjRqA2UWQmqOsWW19K7Iw=", Helpers.TakeScreenshot());

            // Cleanup.
            Helpers.UnloadScene();
        }
    }
}