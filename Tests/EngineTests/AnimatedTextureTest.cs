#region Using

using System.Collections;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.Animation;
using Emotion.Graphics.Assets;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Testing;

#endregion

#nullable enable

namespace Tests.EngineTests;

// This is a legacy test - do not trust it and the comments within
public class AnimatedTextureTest : ProxyRenderTestingScene
{
    // Test whether the animated texture class draws as expected on a 4 frame animations full duration at an interval of 500ms.
    [Test]
    public IEnumerator AnimatedTextureClassDrawing()
    {
        // todo: ONE - modernize and delete animated texture once and for all
        //AssetHandle<TextureAsset> texHandle = Engine.AssetLoader.ONE_Get<TextureAsset>("Images/spritesheetAnimation.png");
        //yield return texHandle;

        //TextureAsset? textureAss = texHandle.Asset;
        //Texture? texture = textureAss?.Texture;
        //Assert.NotNull(texture);

        //var spriteGridFrameSource = new SpriteGridFrameSource(texture.Size, new Vector2(50, 50));
        //var animatedSprite = new AnimatedSprite("Images/spritesheetAnimation.png", spriteGridFrameSource);
        //animatedSprite.Animations.Add("forward", new SpriteAnimationData("forward", 15));
        //var animController = new SpriteAnimationController(animatedSprite);

        var normalLoop = new AnimatedTexture(Engine.AssetLoader.Get<TextureAsset>("Images/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Normal, 500, 0, 3);
        // Test constructor without starting-ending frame as well. It should set the starting and ending frames to 0-3 as well.
        var noLoop = new AnimatedTexture(Engine.AssetLoader.Get<TextureAsset>("Images/spritesheetAnimation.png"), 2, 2, AnimationLoopType.None, 500);
        var normalThenReverse = new AnimatedTexture(Engine.AssetLoader.Get<TextureAsset>("Images/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.NormalThenReverse, 500,
            0, 3);
        var noLoopReverse = new AnimatedTexture(Engine.AssetLoader.Get<TextureAsset>("Images/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.NoneReverse, 500, 0, 3);
        var reverseLoop = new AnimatedTexture(Engine.AssetLoader.Get<TextureAsset>("Images/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Reverse, 500);

        ToRender = (composer) =>
        {
            composer.SetUseViewMatrix(false);

            composer.RenderSprite(new Vector3(10, 10, 0), new Vector2(100, 100), Color.White, normalLoop.Texture, normalLoop.CurrentFrame);
            composer.RenderSprite(new Vector3(115, 10, 0), new Vector2(100, 100), Color.White, noLoop.Texture, noLoop.CurrentFrame);
            composer.RenderSprite(new Vector3(220, 10, 0), new Vector2(100, 100), Color.White, normalThenReverse.Texture, normalThenReverse.CurrentFrame);
            composer.RenderSprite(new Vector3(325, 10, 0), new Vector2(100, 100), Color.White, noLoopReverse.Texture, noLoopReverse.CurrentFrame);
            composer.RenderSprite(new Vector3(430, 10, 0), new Vector2(100, 100), Color.White, reverseLoop.Texture, reverseLoop.CurrentFrame);
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
        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(AnimatedTextureTest), nameof(AnimatedTextureClassDrawing));
        Assert.Equal(0, normalLoop.CurrentFrameIndex);
        Assert.Equal(0, noLoop.CurrentFrameIndex);
        Assert.Equal(0, normalThenReverse.CurrentFrameIndex);
        Assert.Equal(3, noLoopReverse.CurrentFrameIndex);
        Assert.Equal(3, reverseLoop.CurrentFrameIndex);

        // Move 500 ms into the future, as the frames are specified to change every 500ms.
        AdvanceAnimation(500);

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(AnimatedTextureTest), nameof(AnimatedTextureClassDrawing));
        Assert.Equal(1, normalLoop.CurrentFrameIndex);
        Assert.Equal(1, noLoop.CurrentFrameIndex);
        Assert.Equal(1, normalThenReverse.CurrentFrameIndex);
        Assert.Equal(2, noLoopReverse.CurrentFrameIndex);
        Assert.Equal(2, reverseLoop.CurrentFrameIndex);

        // Move 500 ms into the future, as the frames are specified to change every 500ms.
        AdvanceAnimation(500);

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(AnimatedTextureTest), nameof(AnimatedTextureClassDrawing));
        Assert.Equal(2, normalLoop.CurrentFrameIndex);
        Assert.Equal(2, noLoop.CurrentFrameIndex);
        Assert.Equal(2, normalThenReverse.CurrentFrameIndex);
        Assert.Equal(1, noLoopReverse.CurrentFrameIndex);
        Assert.Equal(1, reverseLoop.CurrentFrameIndex);

        // Move 500 ms into the future, as the frames are specified to change every 500ms.
        AdvanceAnimation(500);

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(AnimatedTextureTest), nameof(AnimatedTextureClassDrawing));
        Assert.Equal(3, normalLoop.CurrentFrameIndex);
        Assert.Equal(3, noLoop.CurrentFrameIndex);
        Assert.Equal(3, normalThenReverse.CurrentFrameIndex);
        Assert.Equal(0, noLoopReverse.CurrentFrameIndex);
        Assert.Equal(0, reverseLoop.CurrentFrameIndex);

        // Move 500 ms into the future, as the frames are specified to change every 500ms.
        AdvanceAnimation(500);

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(AnimatedTextureTest), nameof(AnimatedTextureClassDrawing));
        Assert.Equal(0, normalLoop.CurrentFrameIndex);
        Assert.Equal(3, noLoop.CurrentFrameIndex);
        Assert.Equal(2, normalThenReverse.CurrentFrameIndex);
        Assert.Equal(0, noLoopReverse.CurrentFrameIndex);
        Assert.Equal(3, reverseLoop.CurrentFrameIndex);

        // Move 500 ms into the future, as the frames are specified to change every 500ms.
        AdvanceAnimation(500);

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(AnimatedTextureTest), nameof(AnimatedTextureClassDrawing));
        Assert.Equal(1, normalLoop.CurrentFrameIndex);
        Assert.Equal(3, noLoop.CurrentFrameIndex);
        Assert.Equal(1, normalThenReverse.CurrentFrameIndex);
        Assert.Equal(0, noLoopReverse.CurrentFrameIndex);
        Assert.Equal(2, reverseLoop.CurrentFrameIndex);

        AdvanceAnimation(500);
        Assert.Equal(0, normalThenReverse.CurrentFrameIndex);

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

        // Not it should match the starting state
        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(AnimatedTextureTest), nameof(AnimatedTextureClassDrawing));

        Assert.Equal(0, normalLoop.CurrentFrameIndex);
        Assert.Equal(0, noLoop.CurrentFrameIndex);
        Assert.Equal(0, normalThenReverse.CurrentFrameIndex);
        Assert.Equal(3, noLoopReverse.CurrentFrameIndex);
        Assert.Equal(3, reverseLoop.CurrentFrameIndex);

        normalLoop = null;
        noLoop = null;
        normalThenReverse = null;
        noLoopReverse = null;
        reverseLoop = null;
        Engine.AssetLoader.Destroy("Images/spritesheetAnimation.png");
    }

    // Test how the animated texture behaves with invalid inputs and such.
    [Test]
    public IEnumerator AnimatedTextureClassErrorBehavior()
    {
        // The size of the frame is larger than the image itself.
        var wrongFrameSize = new AnimatedTexture(Engine.AssetLoader.Get<TextureAsset>("Images/spritesheetAnimation.png"), new Vector2(125, 50), AnimationLoopType.Normal, 500, 0, 3);
        // Test the auto starting-ending frame constructor working with invalid frame sizes.
        var wrongFrameSizeAltConstructor =
            new AnimatedTexture(Engine.AssetLoader.Get<TextureAsset>("Images/spritesheetAnimation.png"), new Vector2(125, 50), AnimationLoopType.Normal, 500);
        var frameChange = new AnimatedTexture(Engine.AssetLoader.Get<TextureAsset>("Images/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Normal, 500, 0, 3);

        ToRender = (composer) =>
        {
            composer.SetUseViewMatrix(false);

            composer.RenderSprite(new Vector3(10, 10, 0), new Vector2(100, 100), Color.White, wrongFrameSize.Texture, wrongFrameSize.CurrentFrame);
            composer.RenderSprite(new Vector3(115, 10, 0), new Vector2(100, 100), Color.White, wrongFrameSizeAltConstructor.Texture, wrongFrameSizeAltConstructor.CurrentFrame);
            composer.RenderSprite(new Vector3(220, 10, 0), new Vector2(100, 100), Color.White, frameChange.Texture, frameChange.CurrentFrame);
        };

        // The function which advances time for the animation.
        void AdvanceAnimation(float time)
        {
            wrongFrameSize.Update(time);
            wrongFrameSizeAltConstructor.Update(time);
            frameChange.Update(time);
        }

        // Perform unit tests.
        var wrongStartingFrame = new AnimatedTexture(Engine.AssetLoader.Get<TextureAsset>("Images/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Normal, 500, -10, 3);
        Assert.Equal(0, wrongStartingFrame.CurrentFrameIndex);
        wrongStartingFrame.Update(500);
        Assert.Equal(1, wrongStartingFrame.CurrentFrameIndex);
        Assert.Equal(3, wrongStartingFrame.AnimationFrames);
        Assert.Equal(0, wrongStartingFrame.StartingFrame);
        Assert.Equal(3, wrongStartingFrame.EndingFrame);
        Assert.Equal(500, wrongStartingFrame.TimeBetweenFrames);
        wrongStartingFrame = new AnimatedTexture(Engine.AssetLoader.Get<TextureAsset>("Images/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Normal, 500, 4, 3);
        Assert.Equal(0, wrongStartingFrame.CurrentFrameIndex);
        wrongStartingFrame.Update(500);
        Assert.Equal(1, wrongStartingFrame.CurrentFrameIndex);
        Assert.Equal(3, wrongStartingFrame.AnimationFrames);
        Assert.Equal(0, wrongStartingFrame.StartingFrame);
        Assert.Equal(3, wrongStartingFrame.EndingFrame);
        Assert.Equal(500, wrongStartingFrame.TimeBetweenFrames);
        wrongStartingFrame = new AnimatedTexture(Engine.AssetLoader.Get<TextureAsset>("Images/spritesheetAnimation.png"), new Vector2(50, 50), AnimationLoopType.Normal, 500, 3, 2);
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
        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(AnimatedTextureTest), nameof(AnimatedTextureClassErrorBehavior));
        Assert.Equal(0, wrongFrameSize.CurrentFrameIndex);
        Assert.Equal(0, wrongFrameSizeAltConstructor.CurrentFrameIndex);
        Assert.Equal(0, frameChange.CurrentFrameIndex);

        // Move 500 ms into the future, as the frames are specified to change every 500ms.
        AdvanceAnimation(500);

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(AnimatedTextureTest), nameof(AnimatedTextureClassErrorBehavior));
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

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(AnimatedTextureTest), nameof(AnimatedTextureClassErrorBehavior));
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

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(AnimatedTextureTest), nameof(AnimatedTextureClassErrorBehavior));
        Assert.Equal(0, wrongFrameSize.CurrentFrameIndex);
        Assert.Equal(0, wrongFrameSizeAltConstructor.CurrentFrameIndex);
        Assert.Equal(3, frameChange.CurrentFrameIndex);

        // Move 500 ms into the future, as the frames are specified to change every 500ms.
        AdvanceAnimation(500);

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(AnimatedTextureTest), nameof(AnimatedTextureClassErrorBehavior));
        Assert.Equal(0, wrongFrameSize.CurrentFrameIndex);
        Assert.Equal(0, wrongFrameSizeAltConstructor.CurrentFrameIndex);
        Assert.Equal(1, frameChange.CurrentFrameIndex);

        // Move 500 ms into the future, as the frames are specified to change every 500ms.
        AdvanceAnimation(500);

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(AnimatedTextureTest), nameof(AnimatedTextureClassErrorBehavior));
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

        wrongFrameSize = null;
        wrongFrameSizeAltConstructor = null;

        frameChange = null;
        Engine.AssetLoader.Destroy("Images/spritesheetAnimation.png");
    }
}