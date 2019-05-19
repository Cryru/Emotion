#region Using

using System.Numerics;
using Adfectus.Common;
using Adfectus.Primitives;
using Adfectus.Tests.Scenes;
using Xunit;

#endregion

namespace Adfectus.Tests
{
    /// <summary>
    /// Tests connected with the camera functionality of the Renderer module.
    /// </summary>
    [Collection("main")]
    public class Camera
    {
        /// <summary>
        /// Test whether moving the default camera works.
        /// </summary>
        [Fact]
        public void CameraOtherThread()
        {
            // Create scene for this test.
            TestScene extScene = new TestScene
            {
                // Draw a random rectangle.
                ExtDraw = () => { Engine.Renderer.Render(new Vector3(100, 100, 0), new Vector2(10, 10), Color.White); }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Check if what is currently on screen is what is expected.
            Assert.Equal("LMy4DeIV5IfWVgIJZyzoJr7q99S++DWWD4ywPv5M+H8=", Helpers.TakeScreenshot());

            // Move the camera.
            Engine.Renderer.Camera.X -= 50;
            Engine.Renderer.Camera.Y -= 50;

            extScene.WaitFrames(2).Wait();

            // Check if updated as expected.
            Assert.Equal("8jrh3PWG9+ED5C5eTYLYQ8nQ2Oi8CJ5SdMl/Y7Faggs=", Helpers.TakeScreenshot());

            // Restore the camera.
            Engine.Renderer.Camera.X += 50;
            Engine.Renderer.Camera.Y += 50;

            // Cleanup.
            Helpers.UnloadScene();
        }

        /// <summary>
        /// Test whether toggling the camera in the draw allows you to render both on the camera and on the screen.
        /// </summary>
        [Fact]
        public void CameraToggleDraw()
        {
            // Create scene for this test.
            TestScene extScene = new TestScene
            {
                // Move the camera from the default position.
                ExtLoad = () =>
                {
                    Engine.Renderer.Camera.X = -50;
                    Engine.Renderer.Camera.Y = -50;
                },

                // Draw one rectangle on the screen, and one on the camera.
                ExtDraw = () =>
                {
                    Engine.Renderer.ViewMatrixEnabled = false;
                    Engine.Renderer.Render(new Vector3(100, 100, 0), new Vector2(10, 10), Color.Red);
                    Engine.Renderer.ViewMatrixEnabled = true;

                    Engine.Renderer.Render(new Vector3(100, 100, 0), new Vector2(10, 10), Color.White);
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Check if what is currently on screen is what is expected.
            Assert.Equal("gsGfUlBCHDA1KZUQl7epsVxONN5AInsQJyfrE/W3ipE=", Helpers.TakeScreenshot());

            // Move the camera.
            Engine.Renderer.Camera.X -= 50;
            Engine.Renderer.Camera.Y -= 50;

            extScene.WaitFrames(2).Wait();

            // Check if updated as expected.
            Assert.Equal("3RwFL2Gz+XM9Loj9cHwtCZDnnRwfl/7XhvbDuNwZRs4=", Helpers.TakeScreenshot());

            // Restore the camera.
            Engine.Renderer.Camera.X = 0;
            Engine.Renderer.Camera.Y = 0;

            // Cleanup.
            Helpers.UnloadScene();
        }


        /// <summary>
        /// Test whether toggling the camera in the draw allows you to render both on the camera and on the screen.
        /// </summary>
        [Fact]
        public void CameraMoveInDraw()
        {
            // Create scene for this test.
            TestScene extScene = new TestScene
            {
                // Draw one rectangle on the screen, and one on the camera.
                ExtDraw = () =>
                {
                    Engine.Renderer.Camera.X = -50;
                    Engine.Renderer.Camera.Y = -50;
                    Engine.Renderer.Render(new Vector3(100, 100, 0), new Vector2(10, 10), Color.White);

                    Engine.Renderer.Camera.X = 0;
                    Engine.Renderer.Camera.Y = 0;
                    Engine.Renderer.Render(new Vector3(100, 100, 0), new Vector2(10, 10), Color.Blue);
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Check if what is currently on screen is what is expected.
            Assert.Equal("9KuVQ9aRKVhZpjvbD4+Kmhk7L+QwjMFrYLsaar0+Dc8=", Helpers.TakeScreenshot());

            // Restore the camera.
            Engine.Renderer.Camera.X = 0;
            Engine.Renderer.Camera.Y = 0;

            // Cleanup.
            Helpers.UnloadScene();
        }
    }
}