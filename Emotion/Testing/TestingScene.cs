#nullable enable

#region Using

using System.Collections;
using System.IO;
using System.Threading;
using Emotion.Game.Time.Routines;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Scenography;
using Emotion.Standard.Image.PNG;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Testing;

public abstract class TestingScene : Scene
{
    public int RunningTestRoutineIndex = 0;

    protected static FrameBuffer? _screenShotBuffer;
    protected static byte[]? _lastFrameScreenShot;

    public override void UpdateScene(float dt)
    {
        if (ShouldRunLoop())
        {
            TestUpdate();
        }
    }

    public override void RenderScene(RenderComposer composer)
    {
        _screenShotBuffer ??= new FrameBuffer(composer.DrawBuffer.Size).WithColor();
        if (_screenShotBuffer.Size != composer.DrawBuffer.Size) _screenShotBuffer.Resize(composer.DrawBuffer.Size, true);

        if (ShouldRunLoop())
        {
            composer.RenderToAndClear(_screenShotBuffer);

            TestDraw(composer);

            composer.RenderTo(null);
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

    public VerifyScreenshotResult VerifyScreenshot(string? addToScreenshotName = null)
    {
        string fullFunctionName = TestingUtility.GetFunctionBackInStack(1) ?? new Guid().ToString();
        int lastDot = fullFunctionName.LastIndexOf('.');

        string fileName = fullFunctionName;
        if (lastDot != -1) fileName = fileName.Substring(lastDot + 1);
        fileName = fileName.Replace("+MoveNext()", "");
        fileName = AssetLoader.MakeStringPathSafe(fileName);

        var testClass = "TestClass";
        if (lastDot != -1)
        {
            int secondToLastDot = fullFunctionName.LastIndexOf('.', lastDot - 1);
            testClass = fullFunctionName.Substring(secondToLastDot + 1, lastDot - secondToLastDot - 1);
            testClass = testClass.Replace("+MoveNext()", "");
        }

        fileName = $"{RunningTestRoutineIndex} {fileName}";
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
        byte[] screenshot = _lastFrameScreenShot;
        ImageUtil.FlipImageY(screenshot, (int) screenShotSize.Y);

        string screenShotFolder = Path.Join(TestExecutor.TestRunFolder, "Renders");
        screenShotFolder = Path.Join(screenShotFolder, testClass);
        Directory.CreateDirectory(screenShotFolder);

        string screenShotFile = Path.Join(screenShotFolder, $"{fileName}.png");
        byte[] screenShotAsPng = PngFormat.Encode(screenshot, screenShotSize, PixelFormat.Rgba);
        File.WriteAllBytes(screenShotFile, screenShotAsPng);

        // Load reference screenshot.
        var referenceRenderName = $"ReferenceRenders/{testClass}/{fileName}.png";
        var referenceImage = Engine.AssetLoader.Get<OtherAsset>(referenceRenderName, false);
        if (referenceImage == null)
        {
            Assert(false, $"Missing reference image {referenceRenderName}");
            return new VerifyScreenshotResult(false);
        }

        byte[] dataReference = PngFormat.Decode(referenceImage.Content, out PngFileHeader fileHeader);
        if (fileHeader.Size != screenShotSize)
        {
            Assert(false, $"Reference image {referenceRenderName} is of different size than screenshot");
            return new VerifyScreenshotResult(false);
        }

        Assert(dataReference.Length == screenshot.Length);

        Engine.Log.Info($"    Comparing images {fileName}", MessageSource.Test);

        var pixelDifference = new bool[dataReference.Length / 4];
        var differentPixels = 0;
        var totalPixels = 0;
        for (var i = 0; i < dataReference.Length; i += 4)
        {
            byte r = screenshot[i];
            byte g = screenshot[i + 1];
            byte b = screenshot[i + 2];
            byte a = screenshot[i + 3];

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

       
        if (derivationPercent > TestExecutor.PixelDerivationTolerance)
        {
            Engine.Log.Error($"    - Image derivation for {fileName} is too high!", MessageSource.Test);
            return new VerifyScreenshotResult(false);
        }

        return new VerifyScreenshotResult(true);
    }

    protected abstract void TestUpdate();
    protected abstract void TestDraw(RenderComposer c);
    public abstract Func<IEnumerator>[] GetTestCoroutines();

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