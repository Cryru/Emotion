// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Numerics;
using Emotion.Engine;
using Emotion.Game.Animation;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Tests.Interoperability;
using Emotion.Tests.Layers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Emotion.Tests.Tests
{
    /// <summary>
    /// Tests connected with animating textures.
    /// </summary>
    [TestClass]
    public class AnimatingTexture
    {
        /// <summary>
        /// Test whether the animated texture class draws as expected on a 4 frame animations full duration at an interval of
        /// 500ms.
        /// </summary>
        [TestMethod]
        public void AnimatedTextureClassDrawing()
        {
            // Get the host.
            TestHost host = TestInit.TestingHost;

            // References to the animated texture classes.
            AnimatedTexture normalLoop = null;
            AnimatedTexture noLoop = null;
            AnimatedTexture normalThenReverse = null;
            AnimatedTexture noLoopReverse = null;
            AnimatedTexture reverseLoop = null;

            // Create the test layer.
            ExternalLayer extLayer = new ExternalLayer
            {
                // Load the animated texture classes. This also tests loading in another thread.
                ExtLoad = () =>
                {
                    normalLoop = new AnimatedTexture(Context.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Normal, 500, 0, 3);
                    // Test constructor without starting-ending frame as well. It should set the starting and ending frames to 0-3 as well.
                    noLoop = new AnimatedTexture(Context.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.None, 500);
                    normalThenReverse = new AnimatedTexture(Context.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.NormalThenReverse, 500, 0, 3);
                    noLoopReverse = new AnimatedTexture(Context.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.NoneReverse, 500, 0, 3);
                    reverseLoop = new AnimatedTexture(Context.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Reverse, 500);
                },
                // Unload the texture and the animated texture classes.
                ExtUnload = () =>
                {
                    normalLoop = null;
                    noLoop = null;
                    normalThenReverse = null;
                    noLoopReverse = null;
                    reverseLoop = null;
                    Context.AssetLoader.Destroy("Textures/spritesheetAnimation.png");
                },
                ExtUpdate = () =>
                {
                    noLoop.Update(Context.FrameTime);
                    normalLoop.Update(Context.FrameTime);
                    normalThenReverse.Update(Context.FrameTime);
                    noLoopReverse.Update(Context.FrameTime);
                    reverseLoop.Update(Context.FrameTime);
                },
                // Draw the current frames.
                ExtDraw = () =>
                {
                    Context.Renderer.Render(new Vector3(10, 10, 0), new Vector2(100, 100), Color.White, normalLoop.Texture, normalLoop.CurrentFrame);
                    Context.Renderer.Render(new Vector3(115, 10, 0), new Vector2(100, 100), Color.White, noLoop.Texture, noLoop.CurrentFrame);
                    Context.Renderer.Render(new Vector3(220, 10, 0), new Vector2(100, 100), Color.White, normalThenReverse.Texture, normalThenReverse.CurrentFrame);
                    Context.Renderer.Render(new Vector3(325, 10, 0), new Vector2(100, 100), Color.White, noLoopReverse.Texture, noLoopReverse.CurrentFrame);
                    Context.Renderer.Render(new Vector3(430, 10, 0), new Vector2(100, 100), Color.White, reverseLoop.Texture, reverseLoop.CurrentFrame);
                }
            };

            // Add layer.
            Helpers.LoadLayer(extLayer, "texture animation test layer");

            // Assert the objects are as expected.
            Assert.AreEqual(3, normalLoop.AnimationFrames);
            Assert.AreEqual(0, normalLoop.StartingFrame);
            Assert.AreEqual(3, normalLoop.EndingFrame);
            Assert.AreEqual(500, normalLoop.TimeBetweenFrames);

            Assert.AreEqual(3, noLoop.AnimationFrames);
            Assert.AreEqual(0, noLoop.StartingFrame);
            Assert.AreEqual(3, noLoop.EndingFrame);
            Assert.AreEqual(500, noLoop.TimeBetweenFrames);

            Assert.AreEqual(3, normalThenReverse.AnimationFrames);
            Assert.AreEqual(0, normalThenReverse.StartingFrame);
            Assert.AreEqual(3, normalThenReverse.EndingFrame);
            Assert.AreEqual(500, normalThenReverse.TimeBetweenFrames);

            Assert.AreEqual(3, noLoopReverse.AnimationFrames);
            Assert.AreEqual(0, noLoopReverse.StartingFrame);
            Assert.AreEqual(3, noLoopReverse.EndingFrame);
            Assert.AreEqual(500, noLoopReverse.TimeBetweenFrames);

            Assert.AreEqual(3, reverseLoop.AnimationFrames);
            Assert.AreEqual(0, reverseLoop.StartingFrame);
            Assert.AreEqual(3, reverseLoop.EndingFrame);
            Assert.AreEqual(500, reverseLoop.TimeBetweenFrames);

            // Capture starting frames.
            Assert.AreEqual("+V5/7pGLDKeOmu3sn34WU4T2xZa+w2fEhvobywn7Q7c=", host.TakeScreenshot().Hash());
            Assert.AreEqual(0, normalLoop.CurrentFrameIndex);
            Assert.AreEqual(0, noLoop.CurrentFrameIndex);
            Assert.AreEqual(0, normalThenReverse.CurrentFrameIndex);
            Assert.AreEqual(3, noLoopReverse.CurrentFrameIndex);
            Assert.AreEqual(3, reverseLoop.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            // And an additional cycle to ensure buffers are swapped. 
            host.RunCycle(500);
            host.RunCycle();

            Assert.AreEqual("05Ly2inIWVsbzUOrTS8AMVujQ4EB8a12c6VyHVwd7Cg=", host.TakeScreenshot().Hash());
            Assert.AreEqual(1, normalLoop.CurrentFrameIndex);
            Assert.AreEqual(1, noLoop.CurrentFrameIndex);
            Assert.AreEqual(1, normalThenReverse.CurrentFrameIndex);
            Assert.AreEqual(2, noLoopReverse.CurrentFrameIndex);
            Assert.AreEqual(2, reverseLoop.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            // And an additional cycle to ensure buffers are swapped. 
            host.RunCycle(500);
            host.RunCycle();

            Assert.AreEqual("fMI3UqliykZ+X6qvWoEyIrtxXNIng6DpjLlBgoSmWjM=", host.TakeScreenshot().Hash());
            Assert.AreEqual(2, normalLoop.CurrentFrameIndex);
            Assert.AreEqual(2, noLoop.CurrentFrameIndex);
            Assert.AreEqual(2, normalThenReverse.CurrentFrameIndex);
            Assert.AreEqual(1, noLoopReverse.CurrentFrameIndex);
            Assert.AreEqual(1, reverseLoop.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            // And an additional cycle to ensure buffers are swapped. 
            host.RunCycle(500);
            host.RunCycle();

            Assert.AreEqual("3rx9/6sLyHF6l/m0tVa81i7+zLCGyJUNlNld7dDVXNM=", host.TakeScreenshot().Hash());
            Assert.AreEqual(3, normalLoop.CurrentFrameIndex);
            Assert.AreEqual(3, noLoop.CurrentFrameIndex);
            Assert.AreEqual(3, normalThenReverse.CurrentFrameIndex);
            Assert.AreEqual(0, noLoopReverse.CurrentFrameIndex);
            Assert.AreEqual(0, reverseLoop.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            // And an additional cycle to ensure buffers are swapped. 
            host.RunCycle(500);
            host.RunCycle();

            Assert.AreEqual("N4KLiznFPPz67kUGMNI2xdOfUO0xZ6GS13R4gVPSEoo=", host.TakeScreenshot().Hash());
            Assert.AreEqual(0, normalLoop.CurrentFrameIndex);
            Assert.AreEqual(3, noLoop.CurrentFrameIndex);
            Assert.AreEqual(2, normalThenReverse.CurrentFrameIndex);
            Assert.AreEqual(0, noLoopReverse.CurrentFrameIndex);
            Assert.AreEqual(3, reverseLoop.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            // And an additional cycle to ensure buffers are swapped. 
            host.RunCycle(500);
            host.RunCycle();

            Assert.AreEqual("gp+ffxbPoTwwYlWR28U4FlQ4yEJoWwxmcMFo1+Cbykc=", host.TakeScreenshot().Hash());
            Assert.AreEqual(1, normalLoop.CurrentFrameIndex);
            Assert.AreEqual(3, noLoop.CurrentFrameIndex);
            Assert.AreEqual(1, normalThenReverse.CurrentFrameIndex);
            Assert.AreEqual(0, noLoopReverse.CurrentFrameIndex);
            Assert.AreEqual(2, reverseLoop.CurrentFrameIndex);

            // Ensure objects are as expected.
            Assert.AreEqual(3, normalLoop.AnimationFrames);
            Assert.AreEqual(0, normalLoop.StartingFrame);
            Assert.AreEqual(3, normalLoop.EndingFrame);
            Assert.AreEqual(500, normalLoop.TimeBetweenFrames);
            Assert.AreEqual(1, normalLoop.LoopCount);

            Assert.AreEqual(3, noLoop.AnimationFrames);
            Assert.AreEqual(0, noLoop.StartingFrame);
            Assert.AreEqual(3, noLoop.EndingFrame);
            Assert.AreEqual(500, noLoop.TimeBetweenFrames);
            Assert.AreEqual(1, noLoop.LoopCount);

            Assert.AreEqual(3, normalThenReverse.AnimationFrames);
            Assert.AreEqual(0, normalThenReverse.StartingFrame);
            Assert.AreEqual(3, normalThenReverse.EndingFrame);
            Assert.AreEqual(500, normalThenReverse.TimeBetweenFrames);
            Assert.AreEqual(1, normalThenReverse.LoopCount);

            Assert.AreEqual(3, noLoopReverse.AnimationFrames);
            Assert.AreEqual(0, noLoopReverse.StartingFrame);
            Assert.AreEqual(3, noLoopReverse.EndingFrame);
            Assert.AreEqual(500, noLoopReverse.TimeBetweenFrames);
            Assert.AreEqual(1, noLoopReverse.LoopCount);

            Assert.AreEqual(3, reverseLoop.AnimationFrames);
            Assert.AreEqual(0, reverseLoop.StartingFrame);
            Assert.AreEqual(3, reverseLoop.EndingFrame);
            Assert.AreEqual(500, reverseLoop.TimeBetweenFrames);
            Assert.AreEqual(1, reverseLoop.LoopCount);

            // Reset
            normalLoop.Reset();
            noLoop.Reset();
            normalThenReverse.Reset();
            noLoopReverse.Reset();
            reverseLoop.Reset();

            // Ensure starting frame is reset.
            Assert.AreEqual(0, normalLoop.CurrentFrameIndex);
            Assert.AreEqual(0, noLoop.CurrentFrameIndex);
            Assert.AreEqual(0, normalThenReverse.CurrentFrameIndex);
            Assert.AreEqual(3, noLoopReverse.CurrentFrameIndex);
            Assert.AreEqual(3, reverseLoop.CurrentFrameIndex);

            // Ensure objects are as expected.
            Assert.AreEqual(3, normalLoop.AnimationFrames);
            Assert.AreEqual(0, normalLoop.StartingFrame);
            Assert.AreEqual(3, normalLoop.EndingFrame);
            Assert.AreEqual(500, normalLoop.TimeBetweenFrames);
            Assert.AreEqual(0, normalLoop.LoopCount);

            Assert.AreEqual(3, noLoop.AnimationFrames);
            Assert.AreEqual(0, noLoop.StartingFrame);
            Assert.AreEqual(3, noLoop.EndingFrame);
            Assert.AreEqual(500, noLoop.TimeBetweenFrames);
            Assert.AreEqual(0, noLoop.LoopCount);

            Assert.AreEqual(3, normalThenReverse.AnimationFrames);
            Assert.AreEqual(0, normalThenReverse.StartingFrame);
            Assert.AreEqual(3, normalThenReverse.EndingFrame);
            Assert.AreEqual(500, normalThenReverse.TimeBetweenFrames);
            Assert.AreEqual(0, normalThenReverse.LoopCount);

            Assert.AreEqual(3, noLoopReverse.AnimationFrames);
            Assert.AreEqual(0, noLoopReverse.StartingFrame);
            Assert.AreEqual(3, noLoopReverse.EndingFrame);
            Assert.AreEqual(500, noLoopReverse.TimeBetweenFrames);
            Assert.AreEqual(0, noLoopReverse.LoopCount);

            Assert.AreEqual(3, reverseLoop.AnimationFrames);
            Assert.AreEqual(0, reverseLoop.StartingFrame);
            Assert.AreEqual(3, reverseLoop.EndingFrame);
            Assert.AreEqual(500, reverseLoop.TimeBetweenFrames);
            Assert.AreEqual(0, reverseLoop.LoopCount);

            // Run two cycles because of double-buffering.
            host.RunCycle();
            host.RunCycle();

            // Check if matching starting capture.
            Assert.AreEqual("+V5/7pGLDKeOmu3sn34WU4T2xZa+w2fEhvobywn7Q7c=", host.TakeScreenshot().Hash());
            Assert.AreEqual(0, normalLoop.CurrentFrameIndex);
            Assert.AreEqual(0, noLoop.CurrentFrameIndex);
            Assert.AreEqual(0, normalThenReverse.CurrentFrameIndex);
            Assert.AreEqual(3, noLoopReverse.CurrentFrameIndex);
            Assert.AreEqual(3, reverseLoop.CurrentFrameIndex);

            // Cleanup layer.
            Helpers.UnloadLayer(extLayer);

            // Ensure no layers are left loaded.
            Assert.AreEqual(0, Context.LayerManager.LoadedLayers.Length);
        }

        /// <summary>
        /// Test how the animated texture behaves with invalid inputs and such.
        /// </summary>
        [TestMethod]
        public void AnimatedTextureClassErrorBehavior()
        {
            // Get the host.
            TestHost host = TestInit.TestingHost;

            // References to the animated texture classes.
            AnimatedTexture wrongFrameSize = null;
            AnimatedTexture wrongFrameSizeAltConstructor = null;
            AnimatedTexture frameChange = null;

            // Create the test layer.
            ExternalLayer extLayer = new ExternalLayer
            {
                // Load the animated texture classes. This also tests loading in another thread.
                ExtLoad = () =>
                {
                    // The size of the frame is larger than the image itself.
                    wrongFrameSize = new AnimatedTexture(Context.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(125, 50), AnimationLoopType.Normal, 500, 0, 3);
                    // Test the auto starting-ending frame constructor working with invalid frame sizes.
                    wrongFrameSizeAltConstructor = new AnimatedTexture(Context.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(125, 50), AnimationLoopType.Normal, 500);
                    frameChange = new AnimatedTexture(Context.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Normal, 500, 0, 3);
                },
                // Unload the texture and the animated texture classes.
                ExtUnload = () =>
                {
                    wrongFrameSize = null;
                    wrongFrameSizeAltConstructor = null;

                    frameChange = null;
                    Context.AssetLoader.Destroy("Textures/spritesheetAnimation.png");
                },
                ExtUpdate = () =>
                {
                    wrongFrameSize.Update(Context.FrameTime);
                    wrongFrameSizeAltConstructor.Update(Context.FrameTime);
                    frameChange.Update(Context.FrameTime);
                },
                // Draw the current frames.
                ExtDraw = () =>
                {
                    Context.Renderer.Render(new Vector3(10, 10, 0), new Vector2(100, 100), Color.White, wrongFrameSize.Texture, wrongFrameSize.CurrentFrame);
                    Context.Renderer.Render(new Vector3(115, 10, 0), new Vector2(100, 100), Color.White, wrongFrameSizeAltConstructor.Texture, wrongFrameSizeAltConstructor.CurrentFrame);
                    Context.Renderer.Render(new Vector3(220, 10, 0), new Vector2(100, 100), Color.White, frameChange.Texture, frameChange.CurrentFrame);
                }
            };

            // Add layer.
            Helpers.LoadLayer(extLayer, "texture animation test layer");

            // Perform unit tests.
            AnimatedTexture wrongStartingFrame = new AnimatedTexture(Context.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Normal, 500, -10, 3);
            Assert.AreEqual(0, wrongStartingFrame.CurrentFrameIndex);
            wrongStartingFrame.Update(500);
            Assert.AreEqual(1, wrongStartingFrame.CurrentFrameIndex);
            Assert.AreEqual(3, wrongStartingFrame.AnimationFrames);
            Assert.AreEqual(0, wrongStartingFrame.StartingFrame);
            Assert.AreEqual(3, wrongStartingFrame.EndingFrame);
            Assert.AreEqual(500, wrongStartingFrame.TimeBetweenFrames);
            wrongStartingFrame = new AnimatedTexture(Context.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Normal, 500, 4, 3);
            Assert.AreEqual(0, wrongStartingFrame.CurrentFrameIndex);
            wrongStartingFrame.Update(500);
            Assert.AreEqual(1, wrongStartingFrame.CurrentFrameIndex);
            Assert.AreEqual(3, wrongStartingFrame.AnimationFrames);
            Assert.AreEqual(0, wrongStartingFrame.StartingFrame);
            Assert.AreEqual(3, wrongStartingFrame.EndingFrame);
            Assert.AreEqual(500, wrongStartingFrame.TimeBetweenFrames);
            wrongStartingFrame = new AnimatedTexture(Context.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Normal, 500, 3, 2);
            Assert.AreEqual(0, wrongStartingFrame.CurrentFrameIndex);
            wrongStartingFrame.Update(500);
            Assert.AreEqual(1, wrongStartingFrame.CurrentFrameIndex);
            Assert.AreEqual(2, wrongStartingFrame.AnimationFrames);
            Assert.AreEqual(0, wrongStartingFrame.StartingFrame);
            Assert.AreEqual(2, wrongStartingFrame.EndingFrame);
            Assert.AreEqual(500, wrongStartingFrame.TimeBetweenFrames);

            // Assert the objects are as expected.
            Assert.AreEqual(0, wrongFrameSize.AnimationFrames);
            Assert.AreEqual(0, wrongFrameSize.StartingFrame);
            Assert.AreEqual(0, wrongFrameSize.EndingFrame);
            Assert.AreEqual(500, wrongFrameSize.TimeBetweenFrames);

            Assert.AreEqual(0, wrongFrameSizeAltConstructor.AnimationFrames);
            Assert.AreEqual(0, wrongFrameSizeAltConstructor.StartingFrame);
            Assert.AreEqual(0, wrongFrameSizeAltConstructor.EndingFrame);
            Assert.AreEqual(500, wrongFrameSizeAltConstructor.TimeBetweenFrames);

            Assert.AreEqual(3, frameChange.AnimationFrames);
            Assert.AreEqual(0, frameChange.StartingFrame);
            Assert.AreEqual(3, frameChange.EndingFrame);
            Assert.AreEqual(500, frameChange.TimeBetweenFrames);

            // Capture starting frames.
            host.TakeScreenshot().Hash();
            Assert.AreEqual("7dlmbwFfEF061PgLiOscldfVHaRluFw3uYDgXn7iSrs=", host.TakeScreenshot().Hash());
            Assert.AreEqual(0, wrongFrameSize.CurrentFrameIndex);
            Assert.AreEqual(0, wrongFrameSizeAltConstructor.CurrentFrameIndex);
            Assert.AreEqual(0, frameChange.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            // And an additional cycle to ensure buffers are swapped. 
            host.RunCycle(500);
            host.RunCycle();

            Assert.AreEqual("RvVocWwI8CrAz4SQsgg5GEtM2OfmlSZmLajosXnwlPk=", host.TakeScreenshot().Hash());
            Assert.AreEqual(0, wrongFrameSize.CurrentFrameIndex);
            Assert.AreEqual(0, wrongFrameSizeAltConstructor.CurrentFrameIndex);
            Assert.AreEqual(1, frameChange.CurrentFrameIndex);

            // Change starting frame.
            frameChange.StartingFrame = 2;
            Assert.AreEqual(1, frameChange.CurrentFrameIndex);
            host.RunCycle();
            Assert.AreEqual(2, frameChange.CurrentFrameIndex);
            frameChange.StartingFrame = 1;
            host.RunCycle();
            Assert.AreEqual(2, frameChange.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            // And an additional cycle to ensure buffers are swapped. 
            host.RunCycle(500);
            host.RunCycle();

            Assert.AreEqual("/MvDRIhlXTz9bu+wWgnhCeHVbVJAZDMdSUGFcgwA/gk=", host.TakeScreenshot().Hash());
            Assert.AreEqual(0, wrongFrameSize.CurrentFrameIndex);
            Assert.AreEqual(0, wrongFrameSizeAltConstructor.CurrentFrameIndex);
            Assert.AreEqual(3, frameChange.CurrentFrameIndex);

            // Change ending frame.
            frameChange.EndingFrame = 2;
            Assert.AreEqual(3, frameChange.CurrentFrameIndex);
            host.RunCycle();
            Assert.AreEqual(2, frameChange.CurrentFrameIndex);
            frameChange.EndingFrame = 3;
            host.RunCycle();
            Assert.AreEqual(2, frameChange.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            // And an additional cycle to ensure buffers are swapped. 
            host.RunCycle(500);
            host.RunCycle();

            Assert.AreEqual("/MvDRIhlXTz9bu+wWgnhCeHVbVJAZDMdSUGFcgwA/gk=", host.TakeScreenshot().Hash());
            Assert.AreEqual(0, wrongFrameSize.CurrentFrameIndex);
            Assert.AreEqual(0, wrongFrameSizeAltConstructor.CurrentFrameIndex);
            Assert.AreEqual(3, frameChange.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            // And an additional cycle to ensure buffers are swapped. 
            host.RunCycle(500);
            host.RunCycle();

            host.TakeScreenshot().Hash();
            Assert.AreEqual("RvVocWwI8CrAz4SQsgg5GEtM2OfmlSZmLajosXnwlPk=", host.TakeScreenshot().Hash());
            Assert.AreEqual(0, wrongFrameSize.CurrentFrameIndex);
            Assert.AreEqual(0, wrongFrameSizeAltConstructor.CurrentFrameIndex);
            Assert.AreEqual(1, frameChange.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            // And an additional cycle to ensure buffers are swapped. 
            host.RunCycle(500);
            host.RunCycle();

            Assert.AreEqual("NpZUPuYdLTzm0md8REV79sQzLdePj8b26kddLe7/UIE=", host.TakeScreenshot().Hash());
            Assert.AreEqual(0, wrongFrameSize.CurrentFrameIndex);
            Assert.AreEqual(0, wrongFrameSizeAltConstructor.CurrentFrameIndex);
            Assert.AreEqual(2, frameChange.CurrentFrameIndex);

            // Ensure objects are as expected.
            Assert.AreEqual(0, wrongFrameSize.AnimationFrames);
            Assert.AreEqual(0, wrongFrameSize.StartingFrame);
            Assert.AreEqual(0, wrongFrameSize.EndingFrame);
            Assert.AreEqual(500, wrongFrameSize.TimeBetweenFrames);
            Assert.AreEqual(5, wrongFrameSize.LoopCount);

            Assert.AreEqual(0, wrongFrameSizeAltConstructor.AnimationFrames);
            Assert.AreEqual(0, wrongFrameSize.StartingFrame);
            Assert.AreEqual(0, wrongFrameSizeAltConstructor.EndingFrame);
            Assert.AreEqual(500, wrongFrameSizeAltConstructor.TimeBetweenFrames);
            Assert.AreEqual(5, wrongFrameSizeAltConstructor.LoopCount);

            Assert.AreEqual(2, frameChange.AnimationFrames);
            Assert.AreEqual(1, frameChange.StartingFrame);
            Assert.AreEqual(3, frameChange.EndingFrame);
            Assert.AreEqual(500, frameChange.TimeBetweenFrames);
            Assert.AreEqual(1, frameChange.LoopCount);

            // Cleanup layer.
            Helpers.UnloadLayer(extLayer);

            // Ensure no layers are left loaded.
            Assert.AreEqual(0, Context.LayerManager.LoadedLayers.Length);
        }
    }
}