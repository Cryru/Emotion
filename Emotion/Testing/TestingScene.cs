#nullable enable

#region Using

using System.IO;
using Emotion.Core.Systems.IO;
using Emotion.Core.Systems.Logging;
using Emotion.Core.Systems.Scenography;
using Emotion.Standard.Parsers.Image.PNG;
using OpenGL;

#endregion

namespace Emotion.Testing;

public abstract class TestingScene : SceneWithMap
{
    protected static FrameBuffer? _screenShotBuffer;
    protected static byte[]? _lastFrameScreenShot;

    public override void UpdateScene(float dt)
    {
        if (ShouldRunLoop())
        {
            TestUpdate();
        }
    }

    public virtual void BetweenEachTest()
    {

    }

    public override void RenderScene(Renderer composer)
    {
        _screenShotBuffer ??= new FrameBuffer(composer.DrawBuffer.Size).WithColor().WithDepth();
        if (_screenShotBuffer.Size != composer.DrawBuffer.Size) _screenShotBuffer.Resize(composer.DrawBuffer.Size, true);

        if (ShouldRunLoop())
        {
            composer.RenderToAndClear(_screenShotBuffer);

            TestDraw(composer);

            composer.RenderTo(null);
            composer.SetUseViewMatrix(false);
            composer.RenderSprite(Vector3.Zero, _screenShotBuffer.Size, _screenShotBuffer.Texture);

            // We need to sample and store screenshots here as the GLThread tasks are ran at the beginning of a frame
            // rather than end, which means there is no way for a coroutine to obtain it.
            composer.FlushRenderStream();

            FrameBuffer drawBuffer = _screenShotBuffer;
            _lastFrameScreenShot = drawBuffer.Sample(drawBuffer.Viewport, PixelFormat.Rgba);

            OnLoopRan();
        }
    }

    private HashSet<string> _usedNamed = new();

    public VerifyScreenshotResult VerifyScreenshot(string testClass, string fileName, string? addToScreenshotName = null)
    {
        fileName = $"{fileName}()";
        if (addToScreenshotName != null) fileName += addToScreenshotName;
        lock (_usedNamed)
        {
            var counter = 1;
            string originalName = fileName;
            while (_usedNamed.Contains(fileName)) fileName = originalName + "_" + counter++;
            _usedNamed.Add(fileName);
        }

        if (_screenShotBuffer == null || _lastFrameScreenShot == null) return new VerifyScreenshotResult(false);
        Vector2 screenShotSize = _screenShotBuffer.Size;
        byte[] screenShot = _lastFrameScreenShot;
        ImageUtil.FlipImageY(screenShot, (int) screenShotSize.Y);

        static void WriteImageFiles(string testClass, string fileName, byte[] screenShot, Vector2 screenShotSize, ReadOnlySpan<byte> refereceImageSpan)
        {
            string screenShotFolder = Path.Join(TestExecutor.TestRunFolder, "Renders");
            screenShotFolder = Path.Join(screenShotFolder, testClass);
            Directory.CreateDirectory(screenShotFolder);

            string screenShotFilePath = Path.Join(screenShotFolder, $"{fileName}.png");
            byte[] screenShotAsPng = PngFormat.Encode(screenShot, screenShotSize, PixelFormat.Rgba);
            File.WriteAllBytes(screenShotFilePath, screenShotAsPng);

            if (!refereceImageSpan.IsEmpty)
            {
                string referenceFilePath = Path.Join(screenShotFolder, $"{fileName}_reference.png");
                File.WriteAllBytes(referenceFilePath, refereceImageSpan);
            }
        }

        // Load reference screenshot.
        var referenceRenderName = $"ReferenceRenders/{testClass}/{fileName}.png";
        var referenceImage = Engine.AssetLoader.Get<OtherAsset>(referenceRenderName, null, true);
        if (!referenceImage.Loaded)
        {
            WriteImageFiles(testClass, fileName, screenShot, screenShotSize, []);
            Engine.Log.Error($"    - Missing reference image {referenceRenderName}!", MessageSource.Test);
            return new VerifyScreenshotResult(false);
        }

        byte[] dataReference = PngFormat.Decode(referenceImage.Content, out PngFileHeader fileHeader);
        if (fileHeader.Size != screenShotSize)
        {
            Engine.Log.Error($"    - Reference image {referenceRenderName} is of different size than screenshot!", MessageSource.Test);
            return new VerifyScreenshotResult(false);
        }

        Assert(dataReference.Length == screenShot.Length);

        Engine.Log.Info($"    Comparing images {fileName}", MessageSource.Test);

        var pixelDifference = new bool[dataReference.Length / 4];
        var differentPixels = 0;
        var totalPixels = 0;
        for (var i = 0; i < dataReference.Length; i += 4)
        {
            byte r = screenShot[i];
            byte g = screenShot[i + 1];
            byte b = screenShot[i + 2];
            byte a = screenShot[i + 3];

            byte refR = dataReference[i];
            byte refG = dataReference[i + 1];
            byte refB = dataReference[i + 2];
            byte refA = dataReference[i + 3];

            bool different = r != refR || g != refG || b != refB || a != refA;
            if (different) differentPixels++;

            // Don't count empty pixels towards the total.
            if (different || r != 0 || g != 0 || b != 0 || a != 0) totalPixels++;

            pixelDifference[i / 4] = different;
        }

        float derivationPercent = totalPixels == 0 ? 0 : (float) differentPixels / totalPixels;
        derivationPercent *= 100;
        Engine.Log.Info($"    Derivation is {derivationPercent}%", MessageSource.Test);

        if (derivationPercent != 0)
            WriteImageFiles(testClass, fileName, screenShot, screenShotSize, referenceImage.Content.Span);

        if (derivationPercent > TestExecutor.PixelDerivationTolerance)
        {
            Engine.Log.Error($"    - Image derivation for {fileName} is too high!", MessageSource.Test);
            return new VerifyScreenshotResult(false);
        }

        return new VerifyScreenshotResult(true);
    }

    protected abstract void TestUpdate();
    protected abstract void TestDraw(Renderer c);

    // Loop waiter
    private TestWaiterRunLoops? _loopWaiter;
    private static TestingScene? _currentTestingScene;

    public static void SetCurrent(TestingScene? sc)
    {
        _currentTestingScene = sc;
    }

    public static void AddLoopWaiter(TestWaiterRunLoops loopRunWaiter)
    {
        AssertNotNull(_currentTestingScene);
        _currentTestingScene._loopWaiter = loopRunWaiter;
    }

    private bool ShouldRunLoop()
    {
        if (_loopWaiter == null) return false;
        if (_loopWaiter.LoopsToRun == -1) return true;
        if (_loopWaiter.Finished) return false;
        return true;
    }

    private void OnLoopRan()
    {
        _loopWaiter?.AddLoopRan();
    }
}