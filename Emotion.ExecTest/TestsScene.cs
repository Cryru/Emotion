// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Numerics;
using Emotion.Engine;
using Emotion.Engine.Scenography;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Tests;
using Emotion.Tests.Tests;

#endregion

namespace Emotion.ExecTest
{
    public class TestsScene : Scene
    {
        public static void TestMain(string[] args)
        {
            Context.Flags.CrashOnError = false;
            TestInit.Logger = new Debug.Logging.DefaultLogger();

            TestInit.StartTestForeign();
            AnimatingTexture animatedTextureTests = new AnimatingTexture();
            WrapTryCatch(animatedTextureTests.AnimatedTextureClassDrawing);
            WrapTryCatch(animatedTextureTests.AnimatedTextureClassErrorBehavior);
            Assets assetTests = new Assets();
            WrapTryCatch(assetTests.CustomAssetLoader);
            WrapTryCatch(assetTests.EmbeddedAssetLoading);
            Drawing drawingTests = new Drawing();
            WrapTryCatch(drawingTests.AlphaTextureDrawing);
            WrapTryCatch(drawingTests.MapBufferTest);
            WrapTryCatch(drawingTests.TextureDepthTest);
            WrapTryCatch(drawingTests.TextureLoadingAndDrawing);
            Scenography scenographyTests = new Scenography();
            WrapTryCatch(scenographyTests.LayerLightUpdate);
            WrapTryCatch(scenographyTests.SceneLoading);
            Tests.Tests.Sound soundTests = new Tests.Tests.Sound();
            WrapTryCatch(soundTests.FadeIn);
            WrapTryCatch(soundTests.FadeOut);
            WrapTryCatch(soundTests.FocusLossPause);
            WrapTryCatch(soundTests.FocusLossPause);
            WrapTryCatch(soundTests.LoadAndPlaySound);
            WrapTryCatch(soundTests.LoopLastQueue);
            WrapTryCatch(soundTests.LoopQueue);
            WrapTryCatch(soundTests.LoopSingleAndAddingToLoop);
            WrapTryCatch(soundTests.LoopSingleLastOnlyAndAddingToLoop);
            WrapTryCatch(soundTests.LoopSound);
            WrapTryCatch(soundTests.PauseResumeStopSound);
            WrapTryCatch(soundTests.PlayOnPausedLayer);
            WrapTryCatch(soundTests.SoundLayerVolume);
            WrapTryCatch(soundTests.SoundQueue);
            WrapTryCatch(soundTests.SoundQueueAfterFinish);
            WrapTryCatch(soundTests.SoundQueueChannelMix);
            TestInit.TestEnd();
        }

        public static void WrapTryCatch(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                Context.Log.Warning($"Test failed.\n{ex}", Debug.MessageSource.Other);
            }
        }

        public override void Load()
        {
        }

        public override void Update(float frameTime)
        {
        }

        public override void Draw(Renderer renderer)
        {
            Context.Renderer.Render(new Vector3(10, 10, 0), new Vector2(10, 10), Color.White);
        }

        public override void Unload()
        {
        }
    }
}