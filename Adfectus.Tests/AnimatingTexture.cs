#region Using

using System.Numerics;
using Adfectus.Common;
using Adfectus.Game.Animation;
using Adfectus.Graphics;
using Adfectus.Primitives;
using Adfectus.Tests.Scenes;
using Xunit;

#endregion

namespace Adfectus.Tests
{
    /// <summary>
    /// Tests connected with the AnimatedTexture class.
    /// </summary>
    [Collection("main")]
    public class AnimatingTexture
    {
        /// <summary>
        /// Test whether the animated texture class draws as expected on a 4 frame animations full duration at an interval of
        /// 500ms.
        /// </summary>
        [Fact]
        public void AnimatedTextureClassDrawing()
        {
            // References to the animated texture classes.
            AnimatedTexture normalLoop = null;
            AnimatedTexture noLoop = null;
            AnimatedTexture normalThenReverse = null;
            AnimatedTexture noLoopReverse = null;
            AnimatedTexture reverseLoop = null;

            // Create the test scene.
            TestScene extScene = new TestScene
            {
                // Load the animated texture classes. This also tests loading in another thread.
                ExtLoad = () =>
                {
                    normalLoop = new AnimatedTexture(Engine.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Normal, 500, 0, 3);
                    // Test constructor without starting-ending frame as well. It should set the starting and ending frames to 0-3 as well.
                    noLoop = new AnimatedTexture(Engine.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.None, 500);
                    normalThenReverse = new AnimatedTexture(Engine.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.NormalThenReverse, 500, 0, 3);
                    noLoopReverse = new AnimatedTexture(Engine.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.NoneReverse, 500, 0, 3);
                    reverseLoop = new AnimatedTexture(Engine.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Reverse, 500);
                },
                // Unload the texture and the animated texture classes.
                ExtUnload = () =>
                {
                    normalLoop = null;
                    noLoop = null;
                    normalThenReverse = null;
                    noLoopReverse = null;
                    reverseLoop = null;
                    Engine.AssetLoader.Destroy("Textures/spritesheetAnimation.png");
                },
                // Draw the current frames.
                ExtDraw = () =>
                {
                    Engine.Renderer.Render(new Vector3(10, 10, 0), new Vector2(100, 100), Color.White, normalLoop.Texture, normalLoop.CurrentFrame);
                    Engine.Renderer.Render(new Vector3(115, 10, 0), new Vector2(100, 100), Color.White, noLoop.Texture, noLoop.CurrentFrame);
                    Engine.Renderer.Render(new Vector3(220, 10, 0), new Vector2(100, 100), Color.White, normalThenReverse.Texture, normalThenReverse.CurrentFrame);
                    Engine.Renderer.Render(new Vector3(325, 10, 0), new Vector2(100, 100), Color.White, noLoopReverse.Texture, noLoopReverse.CurrentFrame);
                    Engine.Renderer.Render(new Vector3(430, 10, 0), new Vector2(100, 100), Color.White, reverseLoop.Texture, reverseLoop.CurrentFrame);
                }
            };

            // The function which advances time for the animation.
            void AdvanceAnimation(float time)
            {
                noLoop.Update(time);
                normalLoop.Update(time);
                normalThenReverse.Update(time);
                noLoopReverse.Update(time);
                reverseLoop.Update(time);
            }

            // Add scene.
            Helpers.LoadScene(extScene);

            // Assert the objects are as expected.
            Assert.Equal(3, normalLoop.AnimationFrames);
            Assert.Equal(0, normalLoop.StartingFrame);
            Assert.Equal(3, normalLoop.EndingFrame);
            Assert.Equal(500, normalLoop.TimeBetweenFrames);

            Assert.Equal(3, noLoop.AnimationFrames);
            Assert.Equal(0, noLoop.StartingFrame);
            Assert.Equal(3, noLoop.EndingFrame);
            Assert.Equal(500, noLoop.TimeBetweenFrames);

            Assert.Equal(3, normalThenReverse.AnimationFrames);
            Assert.Equal(0, normalThenReverse.StartingFrame);
            Assert.Equal(3, normalThenReverse.EndingFrame);
            Assert.Equal(500, normalThenReverse.TimeBetweenFrames);

            Assert.Equal(3, noLoopReverse.AnimationFrames);
            Assert.Equal(0, noLoopReverse.StartingFrame);
            Assert.Equal(3, noLoopReverse.EndingFrame);
            Assert.Equal(500, noLoopReverse.TimeBetweenFrames);

            Assert.Equal(3, reverseLoop.AnimationFrames);
            Assert.Equal(0, reverseLoop.StartingFrame);
            Assert.Equal(3, reverseLoop.EndingFrame);
            Assert.Equal(500, reverseLoop.TimeBetweenFrames);

            // Capture starting frames.
            Assert.Equal("k5D0Q+X86nekSGHRc+GPriWj3PrW1P3mTdT/pl5JL0k=", Helpers.TakeScreenshot());
            Assert.Equal(0, normalLoop.CurrentFrameIndex);
            Assert.Equal(0, noLoop.CurrentFrameIndex);
            Assert.Equal(0, normalThenReverse.CurrentFrameIndex);
            Assert.Equal(3, noLoopReverse.CurrentFrameIndex);
            Assert.Equal(3, reverseLoop.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            AdvanceAnimation(500);
            extScene.WaitFrames(2).Wait();

            Assert.Equal("+Be5ns3Um0PkLKHS3Rnxvq7l6W3VjPbou40cf5orMSA=", Helpers.TakeScreenshot());
            Assert.Equal(1, normalLoop.CurrentFrameIndex);
            Assert.Equal(1, noLoop.CurrentFrameIndex);
            Assert.Equal(1, normalThenReverse.CurrentFrameIndex);
            Assert.Equal(2, noLoopReverse.CurrentFrameIndex);
            Assert.Equal(2, reverseLoop.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            AdvanceAnimation(500);
            extScene.WaitFrames(2).Wait();

            Assert.Equal("MvPeeizKtrw4CyHazOOwOhHtCsUkriYZa4qKvlSZO4o=", Helpers.TakeScreenshot());
            Assert.Equal(2, normalLoop.CurrentFrameIndex);
            Assert.Equal(2, noLoop.CurrentFrameIndex);
            Assert.Equal(2, normalThenReverse.CurrentFrameIndex);
            Assert.Equal(1, noLoopReverse.CurrentFrameIndex);
            Assert.Equal(1, reverseLoop.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            AdvanceAnimation(500);
            extScene.WaitFrames(2).Wait();

            Assert.Equal("SfwgIjFD0XqInm8dT3tQPVOxMIFsRoHPppiDh1zTRx0=", Helpers.TakeScreenshot());
            Assert.Equal(3, normalLoop.CurrentFrameIndex);
            Assert.Equal(3, noLoop.CurrentFrameIndex);
            Assert.Equal(3, normalThenReverse.CurrentFrameIndex);
            Assert.Equal(0, noLoopReverse.CurrentFrameIndex);
            Assert.Equal(0, reverseLoop.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            AdvanceAnimation(500);
            extScene.WaitFrames(2).Wait();

            Assert.Equal("pdh2KkESxY+TYX4oXOl5DZfYcJL8yWkWefaSlqq9C2A=", Helpers.TakeScreenshot());
            Assert.Equal(0, normalLoop.CurrentFrameIndex);
            Assert.Equal(3, noLoop.CurrentFrameIndex);
            Assert.Equal(2, normalThenReverse.CurrentFrameIndex);
            Assert.Equal(0, noLoopReverse.CurrentFrameIndex);
            Assert.Equal(3, reverseLoop.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            AdvanceAnimation(500);
            extScene.WaitFrames(2).Wait();

            Assert.Equal("5iTPJTbduJvlTlMBWsTeOQ2MuOkQdWkLW70VLt4lZW8=", Helpers.TakeScreenshot());
            Assert.Equal(1, normalLoop.CurrentFrameIndex);
            Assert.Equal(3, noLoop.CurrentFrameIndex);
            Assert.Equal(1, normalThenReverse.CurrentFrameIndex);
            Assert.Equal(0, noLoopReverse.CurrentFrameIndex);
            Assert.Equal(2, reverseLoop.CurrentFrameIndex);

            // Ensure objects are as expected.
            Assert.Equal(3, normalLoop.AnimationFrames);
            Assert.Equal(0, normalLoop.StartingFrame);
            Assert.Equal(3, normalLoop.EndingFrame);
            Assert.Equal(500, normalLoop.TimeBetweenFrames);
            Assert.Equal(1, normalLoop.LoopCount);

            Assert.Equal(3, noLoop.AnimationFrames);
            Assert.Equal(0, noLoop.StartingFrame);
            Assert.Equal(3, noLoop.EndingFrame);
            Assert.Equal(500, noLoop.TimeBetweenFrames);
            Assert.Equal(1, noLoop.LoopCount);

            Assert.Equal(3, normalThenReverse.AnimationFrames);
            Assert.Equal(0, normalThenReverse.StartingFrame);
            Assert.Equal(3, normalThenReverse.EndingFrame);
            Assert.Equal(500, normalThenReverse.TimeBetweenFrames);
            Assert.Equal(1, normalThenReverse.LoopCount);

            Assert.Equal(3, noLoopReverse.AnimationFrames);
            Assert.Equal(0, noLoopReverse.StartingFrame);
            Assert.Equal(3, noLoopReverse.EndingFrame);
            Assert.Equal(500, noLoopReverse.TimeBetweenFrames);
            Assert.Equal(1, noLoopReverse.LoopCount);

            Assert.Equal(3, reverseLoop.AnimationFrames);
            Assert.Equal(0, reverseLoop.StartingFrame);
            Assert.Equal(3, reverseLoop.EndingFrame);
            Assert.Equal(500, reverseLoop.TimeBetweenFrames);
            Assert.Equal(1, reverseLoop.LoopCount);

            // Reset
            normalLoop.Reset();
            noLoop.Reset();
            normalThenReverse.Reset();
            noLoopReverse.Reset();
            reverseLoop.Reset();

            // Ensure starting frame is reset.
            Assert.Equal(0, normalLoop.CurrentFrameIndex);
            Assert.Equal(0, noLoop.CurrentFrameIndex);
            Assert.Equal(0, normalThenReverse.CurrentFrameIndex);
            Assert.Equal(3, noLoopReverse.CurrentFrameIndex);
            Assert.Equal(3, reverseLoop.CurrentFrameIndex);

            // Ensure objects are as expected.
            Assert.Equal(3, normalLoop.AnimationFrames);
            Assert.Equal(0, normalLoop.StartingFrame);
            Assert.Equal(3, normalLoop.EndingFrame);
            Assert.Equal(500, normalLoop.TimeBetweenFrames);
            Assert.Equal(0, normalLoop.LoopCount);

            Assert.Equal(3, noLoop.AnimationFrames);
            Assert.Equal(0, noLoop.StartingFrame);
            Assert.Equal(3, noLoop.EndingFrame);
            Assert.Equal(500, noLoop.TimeBetweenFrames);
            Assert.Equal(0, noLoop.LoopCount);

            Assert.Equal(3, normalThenReverse.AnimationFrames);
            Assert.Equal(0, normalThenReverse.StartingFrame);
            Assert.Equal(3, normalThenReverse.EndingFrame);
            Assert.Equal(500, normalThenReverse.TimeBetweenFrames);
            Assert.Equal(0, normalThenReverse.LoopCount);

            Assert.Equal(3, noLoopReverse.AnimationFrames);
            Assert.Equal(0, noLoopReverse.StartingFrame);
            Assert.Equal(3, noLoopReverse.EndingFrame);
            Assert.Equal(500, noLoopReverse.TimeBetweenFrames);
            Assert.Equal(0, noLoopReverse.LoopCount);

            Assert.Equal(3, reverseLoop.AnimationFrames);
            Assert.Equal(0, reverseLoop.StartingFrame);
            Assert.Equal(3, reverseLoop.EndingFrame);
            Assert.Equal(500, reverseLoop.TimeBetweenFrames);
            Assert.Equal(0, reverseLoop.LoopCount);

            // For any changes to occur a cycle needs to be ran.
            extScene.WaitFrames(2).Wait();

            // Check if matching starting capture.
            Assert.Equal("k5D0Q+X86nekSGHRc+GPriWj3PrW1P3mTdT/pl5JL0k=", Helpers.TakeScreenshot());
            Assert.Equal(0, normalLoop.CurrentFrameIndex);
            Assert.Equal(0, noLoop.CurrentFrameIndex);
            Assert.Equal(0, normalThenReverse.CurrentFrameIndex);
            Assert.Equal(3, noLoopReverse.CurrentFrameIndex);
            Assert.Equal(3, reverseLoop.CurrentFrameIndex);

            // Cleanup.
            Helpers.UnloadScene();
        }

        /// <summary>
        /// Test how the animated texture behaves with invalid inputs and such.
        /// </summary>
        [Fact]
        public void AnimatedTextureClassErrorBehavior()
        {
            // References to the animated texture classes.
            AnimatedTexture wrongFrameSize = null;
            AnimatedTexture wrongFrameSizeAltConstructor = null;
            AnimatedTexture frameChange = null;

            // Create the test scene.
            TestScene extScene = new TestScene
            {
                // Load the animated texture classes. This also tests loading in another thread.
                ExtLoad = () =>
                {
                    // The size of the frame is larger than the image itself.
                    wrongFrameSize = new AnimatedTexture(Engine.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(125, 50), AnimationLoopType.Normal, 500, 0, 3);
                    // Test the auto starting-ending frame constructor working with invalid frame sizes.
                    wrongFrameSizeAltConstructor = new AnimatedTexture(Engine.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(125, 50), AnimationLoopType.Normal, 500);
                    frameChange = new AnimatedTexture(Engine.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Normal, 500, 0, 3);
                },
                // Unload the texture and the animated texture classes.
                ExtUnload = () =>
                {
                    wrongFrameSize = null;
                    wrongFrameSizeAltConstructor = null;

                    frameChange = null;
                    Engine.AssetLoader.Destroy("Textures/spritesheetAnimation.png");
                },
                // Draw the current frames.
                ExtDraw = () =>
                {
                    Engine.Renderer.Render(new Vector3(10, 10, 0), new Vector2(100, 100), Color.White, wrongFrameSize.Texture, wrongFrameSize.CurrentFrame);
                    Engine.Renderer.Render(new Vector3(115, 10, 0), new Vector2(100, 100), Color.White, wrongFrameSizeAltConstructor.Texture, wrongFrameSizeAltConstructor.CurrentFrame);
                    Engine.Renderer.Render(new Vector3(220, 10, 0), new Vector2(100, 100), Color.White, frameChange.Texture, frameChange.CurrentFrame);
                }
            };

            // The function which advances time for the animation.
            void AdvanceAnimation(float time)
            {
                wrongFrameSize.Update(time);
                wrongFrameSizeAltConstructor.Update(time);
                frameChange.Update(time);
            }

            // Add scene.
            Helpers.LoadScene(extScene);

            // Perform unit tests.
            AnimatedTexture wrongStartingFrame = new AnimatedTexture(Engine.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Normal, 500, -10, 3);
            Assert.Equal(0, wrongStartingFrame.CurrentFrameIndex);
            wrongStartingFrame.Update(500);
            Assert.Equal(1, wrongStartingFrame.CurrentFrameIndex);
            Assert.Equal(3, wrongStartingFrame.AnimationFrames);
            Assert.Equal(0, wrongStartingFrame.StartingFrame);
            Assert.Equal(3, wrongStartingFrame.EndingFrame);
            Assert.Equal(500, wrongStartingFrame.TimeBetweenFrames);
            wrongStartingFrame = new AnimatedTexture(Engine.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Normal, 500, 4, 3);
            Assert.Equal(0, wrongStartingFrame.CurrentFrameIndex);
            wrongStartingFrame.Update(500);
            Assert.Equal(1, wrongStartingFrame.CurrentFrameIndex);
            Assert.Equal(3, wrongStartingFrame.AnimationFrames);
            Assert.Equal(0, wrongStartingFrame.StartingFrame);
            Assert.Equal(3, wrongStartingFrame.EndingFrame);
            Assert.Equal(500, wrongStartingFrame.TimeBetweenFrames);
            wrongStartingFrame = new AnimatedTexture(Engine.AssetLoader.Get<Texture>("Textures/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Normal, 500, 3, 2);
            Assert.Equal(0, wrongStartingFrame.CurrentFrameIndex);
            wrongStartingFrame.Update(500);
            Assert.Equal(1, wrongStartingFrame.CurrentFrameIndex);
            Assert.Equal(2, wrongStartingFrame.AnimationFrames);
            Assert.Equal(0, wrongStartingFrame.StartingFrame);
            Assert.Equal(2, wrongStartingFrame.EndingFrame);
            Assert.Equal(500, wrongStartingFrame.TimeBetweenFrames);

            // Assert the objects are as expected.
            Assert.Equal(0, wrongFrameSize.AnimationFrames);
            Assert.Equal(0, wrongFrameSize.StartingFrame);
            Assert.Equal(0, wrongFrameSize.EndingFrame);
            Assert.Equal(500, wrongFrameSize.TimeBetweenFrames);

            Assert.Equal(0, wrongFrameSizeAltConstructor.AnimationFrames);
            Assert.Equal(0, wrongFrameSizeAltConstructor.StartingFrame);
            Assert.Equal(0, wrongFrameSizeAltConstructor.EndingFrame);
            Assert.Equal(500, wrongFrameSizeAltConstructor.TimeBetweenFrames);

            Assert.Equal(3, frameChange.AnimationFrames);
            Assert.Equal(0, frameChange.StartingFrame);
            Assert.Equal(3, frameChange.EndingFrame);
            Assert.Equal(500, frameChange.TimeBetweenFrames);

            // Capture starting frames.
            Assert.Equal("BE+gi0MdZrqWI2S3owCkiS1evwbyTlPxZtdwwJDwQn4=", Helpers.TakeScreenshot());
            Assert.Equal(0, wrongFrameSize.CurrentFrameIndex);
            Assert.Equal(0, wrongFrameSizeAltConstructor.CurrentFrameIndex);
            Assert.Equal(0, frameChange.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            AdvanceAnimation(500);
            extScene.WaitFrames(2).Wait();

            Assert.Equal("Fwq9SouHAzrTtWoa24vZCaogWoKlh2Z38h2L17lNNw4=", Helpers.TakeScreenshot());
            Assert.Equal(0, wrongFrameSize.CurrentFrameIndex);
            Assert.Equal(0, wrongFrameSizeAltConstructor.CurrentFrameIndex);
            Assert.Equal(1, frameChange.CurrentFrameIndex);

            // Change starting frame.
            frameChange.StartingFrame = 2;
            Assert.Equal(1, frameChange.CurrentFrameIndex);
            AdvanceAnimation(0);
            Assert.Equal(2, frameChange.CurrentFrameIndex);
            frameChange.StartingFrame = 1;
            AdvanceAnimation(0);
            Assert.Equal(2, frameChange.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            AdvanceAnimation(500);
            extScene.WaitFrames(2).Wait();

            Assert.Equal("sqFDVON43bN6e0fbCkKIKLRQVNq1KxknUjUs4mY04Xw=", Helpers.TakeScreenshot());
            Assert.Equal(0, wrongFrameSize.CurrentFrameIndex);
            Assert.Equal(0, wrongFrameSizeAltConstructor.CurrentFrameIndex);
            Assert.Equal(3, frameChange.CurrentFrameIndex);

            // Change ending frame.
            frameChange.EndingFrame = 2;
            Assert.Equal(3, frameChange.CurrentFrameIndex);
            AdvanceAnimation(0);
            Assert.Equal(2, frameChange.CurrentFrameIndex);
            frameChange.EndingFrame = 3;
            AdvanceAnimation(0);
            Assert.Equal(2, frameChange.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            AdvanceAnimation(500);
            extScene.WaitFrames(2).Wait();

            Assert.Equal("sqFDVON43bN6e0fbCkKIKLRQVNq1KxknUjUs4mY04Xw=", Helpers.TakeScreenshot());
            Assert.Equal(0, wrongFrameSize.CurrentFrameIndex);
            Assert.Equal(0, wrongFrameSizeAltConstructor.CurrentFrameIndex);
            Assert.Equal(3, frameChange.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            AdvanceAnimation(500);
            extScene.WaitFrames(2).Wait();

            Assert.Equal("Fwq9SouHAzrTtWoa24vZCaogWoKlh2Z38h2L17lNNw4=", Helpers.TakeScreenshot());
            Assert.Equal(0, wrongFrameSize.CurrentFrameIndex);
            Assert.Equal(0, wrongFrameSizeAltConstructor.CurrentFrameIndex);
            Assert.Equal(1, frameChange.CurrentFrameIndex);

            // Move 500 ms into the future, as the frames are specified to change every 500ms.
            AdvanceAnimation(500);
            extScene.WaitFrames(2).Wait();

            Assert.Equal("9RHVfW82y/uvEHsI7GMqzg1m2N7tuOyoZHtmkymSlLE=", Helpers.TakeScreenshot());
            Assert.Equal(0, wrongFrameSize.CurrentFrameIndex);
            Assert.Equal(0, wrongFrameSizeAltConstructor.CurrentFrameIndex);
            Assert.Equal(2, frameChange.CurrentFrameIndex);

            // Ensure objects are as expected.
            Assert.Equal(0, wrongFrameSize.AnimationFrames);
            Assert.Equal(0, wrongFrameSize.StartingFrame);
            Assert.Equal(0, wrongFrameSize.EndingFrame);
            Assert.Equal(500, wrongFrameSize.TimeBetweenFrames);
            Assert.Equal(5, wrongFrameSize.LoopCount);

            Assert.Equal(0, wrongFrameSizeAltConstructor.AnimationFrames);
            Assert.Equal(0, wrongFrameSize.StartingFrame);
            Assert.Equal(0, wrongFrameSizeAltConstructor.EndingFrame);
            Assert.Equal(500, wrongFrameSizeAltConstructor.TimeBetweenFrames);
            Assert.Equal(5, wrongFrameSizeAltConstructor.LoopCount);

            Assert.Equal(2, frameChange.AnimationFrames);
            Assert.Equal(1, frameChange.StartingFrame);
            Assert.Equal(3, frameChange.EndingFrame);
            Assert.Equal(500, frameChange.TimeBetweenFrames);
            Assert.Equal(1, frameChange.LoopCount);

            // Cleanup.
            Helpers.UnloadScene();
        }
    }
}