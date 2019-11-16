#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Standard.Image.PNG;
using Emotion.Standard.Logging;
using Emotion.Test.Helpers;
using Emotion.Test.Results;

#endregion

namespace Emotion.Test
{
    public static class Runner
    {
        /// <summary>
        /// The logger for this runner.
        /// </summary>
        public static LoggingProvider Log;

        /// <summary>
        /// The folder when rendered results are stored.
        /// </summary>
        public static string RenderResultStorage = "ReferenceImages";

        /// <summary>
        /// The id of the runner instance.
        /// </summary>
        public static int RunnerId { get; private set; } = Process.GetCurrentProcess().Id;

        /// <summary>
        /// Whether to not run linked runners.
        /// </summary>
        public static bool NoLinkedRunners;

        /// <summary>
        /// The id of the test run. This is usually the id of the runner who initiated the test.
        /// </summary>
        public static string TestRunId { get; private set; }

        /// <summary>
        /// The folder the results of the test are to be stored in.
        /// </summary>
        public static string TestRunFolder { get; private set; }

        /// <summary>
        /// The folder to store reference images in for this runner and test run.
        /// </summary>
        public static string RunnerReferenceImageFolder { get; private set; }

        /// <summary>
        /// Whether executing test classes with a certain tag only.
        /// </summary>
        public static string TestTag;

        /// <summary>
        /// What percentage of the whole image's pixels can be different.
        /// </summary>
        public static float PixelDerivationTolerance = 10;

        private static Action<float> _loopAction;
        private static ManualResetEvent _loopWaiter;
        private static int _loopCounter;

        private static List<LinkedRunner> _linkedRunners = new List<LinkedRunner>();
        private static Regex _testCompletedRegex = new Regex(@"(?:Test completed: )([0-9]+?)\/([0-9]*)(?:!)");

        private static bool _runnerFolderCreated;

        /// <summary>
        /// Other configs which can be referenced in linked mode.
        /// </summary>
        private static Dictionary<string, Action<Configurator>> _otherConfigs = new Dictionary<string, Action<Configurator>>
        {
            {"compat", (c) => c.SetRenderSettings(true)},
            {"fullScale", (c) => c.SetRenderSize()},
            {"marginScale", (c) => c.SetRenderSize(null, false, false)},
            {"marginScaleInteger", (c) => c.SetRenderSize(null, true, false)},
        };

        /// <summary>
        /// Other runners to run. and the arguments to run them with.
        ///
        /// conf - Means that a config from _otherConfigs with that key will be used for initializing the engine.
        /// testOnly - Means that the engine will not be initialized. Used for running unit tests which don't depend on the engine.
        /// tag - Means that only tests with this tag will be run. If the tests are set to "tagOnly" that means that they will be run only if filtered by tag.
        /// </summary>
        private static List<string> _otherRunners = new List<string>
        {
            "conf=compat",
            "tag=EmotionDesktop testOnly",
            "tag=Assets",
            "tag=Scripting",
            "tag=Coroutine",
            "tag=FullScale conf=fullScale",
            "tag=StandardAudio"
        };

        private static void Main(string[] args)
        {
            // Correct the startup directory to the directory of the executable.
            // Emotion also does this inside the engine setup.
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            ResultDb.LoadCache();

            // Check for test run id. This signifies whether the runner is linked.
            TestRunId = ArgumentsParser.FindArgument(args, "testRunId=", out string testRunId) ? testRunId : RunnerId.ToString();
            TestRunFolder = Path.Join("TestResults", $"{TestRunId}");
            RunnerReferenceImageFolder = Path.Join(TestRunFolder, RenderResultStorage, $"LR{RunnerId}References");

            // Check if master runner, and run linked runners.
            if (TestRunId == RunnerId.ToString())
            {
                if (!NoLinkedRunners)
                {
                    // Spawn a runner for each runtime config.
                    foreach (string c in _otherRunners)
                    {
                        _linkedRunners.Add(new LinkedRunner(c));
                    }
                }

                Log = new DefaultLogger(true, Path.Join(TestRunFolder, "Logs"));
            }
            else
            {
                Log = new LinkedRunnerLogger();
                Log.Info($"I am a linked runner with arguments {string.Join(" ", args)}", CustomMSource.TestRunner);
            }

            // Check if running only specific tests.
            if (ArgumentsParser.FindArgument(args, "tag=", out string testTag)) TestTag = testTag;

            // Check if running tests without an engine instance - this shouldn't be used with a tag because most tests except an instance.
            if (ArgumentsParser.FindArgument(args, "testOnly", out string _))
            {
                Task tests = Task.Run(RunTests);
                while (!tests.IsCompleted) TestLoop();
                return;
            }

            // Set the default engine settings for the test runner.
            // The resolution is set like that because it is the resolution of the render references.
            Configurator config = new Configurator().SetDebug(true, true, TestLoop).SetLogger(Log).SetHostSettings(new Vector2(640, 360)).SetRenderSize(fullScale: false);
            // Check if running with a custom config in linked mode.
            if (ArgumentsParser.FindArgument(args, "conf=", out string linkedModeConfig))
                if (_otherConfigs.ContainsKey(linkedModeConfig))
                    _otherConfigs[linkedModeConfig](config);

            Engine.Setup(config);
            // Move the camera center in a way that its center is 0,0
            Engine.Renderer.Camera.Position += new Vector3(Engine.Renderer.Camera.WorldToScreen(Vector2.Zero), 0);
            Task.Run(() =>
            {
                // Wait for the engine to start.
                while (!Engine.Running && !Engine.Stopped)
                {
                }

                // If crashed.
                if (Engine.Stopped) return;

                // Name the thread.
                if (Thread.CurrentThread.Name == null) Thread.CurrentThread.Name = "Runner Thread";

                RunTests();

                Engine.Quit();

                // Wait for the engine to stop.
                while (Engine.Running)
                {
                }
            });
            Engine.Run();
        }

        private static bool TestLoop()
        {
            try
            {
                // Check if a tick substitute is found.
                if (_loopAction != null)
                {
                    try
                    {
                        _loopAction.Invoke(Engine.DeltaTime);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"LoopAction Error - {ex}", MessageSource.Other);
                    }

                    _loopCounter--;

                    if (_loopCounter > 0) return false;
                    _loopAction = null;

                    // Release the lock.
                    _loopWaiter.Set();
                    _loopWaiter = null;
                }

                // Check if running loops.
                if (_loopCounter <= 0) return false;
                _loopCounter--;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Test runner encountered error in loop - {ex}", MessageSource.Other);
                Debug.Assert(false);
                return false;
            }
        }

        private static void RunTests()
        {
            // Find all test classes.
            Type[] testClasses = typeof(Runner).Assembly.GetTypes().AsParallel().Where(x => x.GetCustomAttributes(typeof(TestAttribute), true).Length > 0).ToArray();

            var timeTracker = new Stopwatch();
            var testCount = 0;
            var failedTests = 0;

            foreach (Type classType in testClasses)
            {
                // Check if filtering by tag.
                var t = (TestAttribute) classType.GetCustomAttributes(typeof(TestAttribute), true).FirstOrDefault();
                if (!string.IsNullOrEmpty(TestTag) && t?.Tag != TestTag)
                {
                    Log.Info($"Skipping class {classType} because it doesn't match tag filter '{TestTag}'.", CustomMSource.TestRunner);
                    continue;
                }

                if (string.IsNullOrEmpty(TestTag) && (t?.TagOnly ?? false))
                {
                    Log.Info($"Skipping class {classType} because it will only run with the tag '{t.Tag}'.", CustomMSource.TestRunner);
                    continue;
                }

                Log.Info($"Running test class {classType}...", CustomMSource.TestRunner);

                // Create an instance of the test class.
                object testClassInstance = Activator.CreateInstance(classType);

                // Find all test functions in this class.
                MethodInfo[] functions = classType.GetMethods().AsParallel().Where(x => x.GetCustomAttributes(typeof(TestAttribute), true).Length > 0).ToArray();
                float classTimer = 0;

                foreach (MethodInfo func in functions)
                {
                    Log.Info($"  Running test {func.Name}...", CustomMSource.TestRunner);
                    timeTracker.Restart();
                    testCount++;
                    try
                    {
                        func.Invoke(testClassInstance, new object[] { });
                    }
                    catch (ImageDerivationException)
                    {
                        failedTests++;
                    }
                    catch (Exception ex)
                    {
                        failedTests++;
                        if (ex.InnerException is ImageDerivationException)
                        {
                            Log.Error($"{ex.InnerException.Message}", CustomMSource.TestRunner);
                            continue;
                        }
                        Log.Error($" Test {func.Name} failed - {ex}", CustomMSource.TestRunner);
                        Debug.Assert(false);
                    }

                    Log.Info($"  Test {func.Name} completed in {timeTracker.ElapsedMilliseconds}ms!", CustomMSource.TestRunner);
                    classTimer += timeTracker.ElapsedMilliseconds;
                }

                Log.Info($"Test class {classType} completed in {classTimer}ms!", CustomMSource.TestRunner);
            }

            Log.Info($"Test completed: {testCount - failedTests}/{testCount}!", CustomMSource.TestRunner);
            var results = new List<string> {$"Master: Test completed: {testCount - failedTests}/{testCount}!"};

            // Wait for linked runners to exit.
            foreach (LinkedRunner linked in _linkedRunners)
            {
                Log.Info("----------------------------------------------------------------------", CustomMSource.TestRunner);
                Log.Info($"Waiting for LR{linked.Id} - ({linked.Args})", CustomMSource.TestRunner);
                int exitCode = linked.WaitForFinish(out string output, out string errorOutput);
                output = output.Trim();
                errorOutput = errorOutput.Trim();

                // Try to find the test completed line.
                Match match = _testCompletedRegex.Match(output);
                if (match.Success)
                {
                    try
                    {
                        int testsSuccess = int.Parse(match.Groups[1].Value);
                        int testsRun = int.Parse(match.Groups[2].Value);

                        int failed = testsRun - testsSuccess;
                        failedTests += failed;
                        testCount += testsRun;

                        results.Add($"LR{linked.Id} ({linked.Args}) {match.Groups[0].Value} {(!string.IsNullOrEmpty(errorOutput) ? "ERR" : "")}");
                    }
                    catch (Exception)
                    {
                        Log.Info($"Couldn't read tests completed from LR{linked.Id}.", CustomMSource.TestRunner);
                    }
                }
                else
                {
                    results.Add($"LR{linked.Id} ({linked.Args}) <Unknown>/<Unknown> {(!string.IsNullOrEmpty(errorOutput) ? "ERR" : "")}");
                }

                Log.Info($"LR{linked.Id} exited with code {exitCode}.", CustomMSource.TestRunner);
                Log.Info($"Dumping log from LR{linked.Id}\n{output}", CustomMSource.TestRunner);
                if (!string.IsNullOrEmpty(errorOutput)) Log.Info($"[LR{linked.Id}] Error Output\n{errorOutput}", CustomMSource.TestRunner);
            }

            // Post final results if master.
            if (TestRunId != RunnerId.ToString()) return;
            Log.Info($"Final test results: {testCount - failedTests}/{testCount}!", CustomMSource.TestRunner);
            foreach (string r in results)
            {
                Log.Info($"     {r}", CustomMSource.TestRunner);
            }
        }

        /// <summary>
        /// Execute the action as if it was in the main loop.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="times">The number of times to execute.</param>
        /// <returns>A token to await the execution on.</returns>
        public static ManualResetEvent ExecuteAsLoop(Action<float> action, int times = 1)
        {
            _loopAction = action;
            _loopWaiter = new ManualResetEvent(false);
            _loopCounter = times;
            return _loopWaiter;
        }

        /// <summary>
        /// Run the loop a number of times.
        /// The loop is prevented from running by default (by returning bool - false), by calling this
        /// you will allow the loop to be ran a number of times.
        /// </summary>
        /// <param name="count">The number of times to run the loop.</param>
        public static void RunLoop(int count = 1)
        {
            _loopCounter = count;
        }

        /// <summary>
        /// Used for debugging a specific linked runner.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="args"></param>
        private static void RunAsRunner(string text, ref string[] args)
        {
            NoLinkedRunners = true;
            args = text.Split(" ");
        }

        #region Image Comparison

        /// <summary>
        /// Verify what has been rendered with a predefined render result.
        /// </summary>
        /// <param name="renderId">The cached id to check against.</param>
        public static void VerifyScreenshot(string renderId)
        {
            // Take a screenshot to compare to the expected image. Assume the sizes are the same.
            byte[] screenshot = Engine.Renderer.GetScreenshot(out Vector2 screenShotSize);
            VerifyCachedRender(renderId, screenshot, screenShotSize);
        }

        /// <summary>
        /// Compare a cached render against another image.
        /// </summary>
        /// <param name="renderId">The cached id to check against.</param>
        /// <param name="imageToCompareAgainst">The image to compare to it.</param>
        /// <param name="imageSize">The size of the image. The render's size must match the comparison size.</param>
        public static void VerifyCachedRender(string renderId, byte[] imageToCompareAgainst, Vector2 imageSize)
        {
            byte[] cachedRender = null;
            if (ResultDb.CachedResults.ContainsKey(renderId))
                cachedRender = ResultDb.CachedResults[renderId];
            else
                Log.Warning($"      Missing comparison render with id {renderId}.", CustomMSource.TestRunner);

            VerifyImages(renderId, cachedRender, imageToCompareAgainst, imageSize);
        }

        /// <summary>
        /// Verify two images.
        /// </summary>
        /// <param name="compareName">
        /// The name of the comparison. Should be unique for all tests. Is used to store information on
        /// the disk about the comparison.
        /// </param>
        /// <param name="originalImage">The original image. Must be in a four component format.</param>
        /// <param name="comparisonImage">The image comparing it to. Must be in a four component format.</param>
        /// <param name="comparisonSize">The size of the second image.</param>
        public static void VerifyImages(string compareName, byte[] originalImage, byte[] comparisonImage, Vector2 comparisonSize)
        {
            // Runner reference image folders should be created only for runners who verify images.
            if(!_runnerFolderCreated)
            {
                Directory.CreateDirectory(RunnerReferenceImageFolder);
                _runnerFolderCreated = true;
            }

            // Invent a name for this comparison and a folder to store data in, in case it is derived.
            string fileName = $"{compareName}.png";

            // We want to store the comparison image for possible manual comparison.
            string filePath = Path.Join(RunnerReferenceImageFolder, fileName);
            byte[] file = PngFormat.Encode(comparisonImage, (int) comparisonSize.X, (int) comparisonSize.Y);
            File.WriteAllBytes(filePath, file);

            // Check if the original image is missing, in which case we just store the comparison image.
            if (originalImage == null) return;

            Log.Info($"      Comparing images {compareName}...", CustomMSource.TestRunner);

            float derivedPixelPercentage;
            byte[] derivationImage = null;

            // If the size isn't the same, it is all derived.
            if (originalImage.Length == comparisonImage.Length)
                derivedPixelPercentage = CalculateImageDerivation(originalImage, comparisonImage, out derivationImage) * 100;
            else
                derivedPixelPercentage = 100;

            if (derivedPixelPercentage == 0)
            {
                Log.Info("          No derivation.", CustomMSource.TestRunner);
                return;
            }

            // Save a derivation image showing the differences.
            if (derivationImage != null)
            {
                string directory = Path.Join(RunnerReferenceImageFolder, $"Comparison_{fileName}");
                Directory.CreateDirectory(directory);
                byte[] derivedFile = PngFormat.Encode(derivationImage, (int) comparisonSize.X, (int) comparisonSize.Y);
                File.WriteAllBytes(Path.Join(directory, "derivation.png"), derivedFile);
            }

            // Assert derivation is not higher than tolerable. This is not done using the Emotion.Test assert so it doesn't stop the test from continuing.
            if (derivedPixelPercentage > PixelDerivationTolerance)
            {
                throw new ImageDerivationException($"          Failed derivation check. Derivation is {derivedPixelPercentage}%.");
            }
            Log.Info($"          Derivation is {derivedPixelPercentage}%.", CustomMSource.TestRunner);
        }

        /// <summary>
        /// Calculate the percentage pixels have derived between two images.
        /// </summary>
        /// <param name="originalImage">
        /// The original image. Must be in a four component format, for best results alpha should be
        /// the last component.
        /// </param>
        /// <param name="newImage">
        /// The derived image. Must be in a four component format, for best results alpha should be the last
        /// component.
        /// </param>
        /// <param name="derivationImage">
        /// An image displaying the derivations. RGBA format. Blue where alpha doesn't match (or blue
        /// component doesn't match) and RG where they don't match.
        /// </param>
        /// <returns>The percentage of pixels derivation between the two images.</returns>
        public static float CalculateImageDerivation(byte[] originalImage, byte[] newImage, out byte[] derivationImage)
        {
            derivationImage = new byte[originalImage.Length];

            var emptyPixels = 0;
            var differentComponents = 0;
            for (var i = 0; i < originalImage.Length; i += 4)
            {
                byte r = originalImage[i];
                byte g = originalImage[i + 1];
                byte b = originalImage[i + 2];
                byte a = originalImage[i + 3];

                var anyDerivation = false;

                if (r != newImage[i])
                {
                    derivationImage[i] = 255;
                    anyDerivation = true;
                    differentComponents++;
                }

                if (g != newImage[i + 1])
                {
                    derivationImage[i + 1] = 255;
                    anyDerivation = true;
                    differentComponents++;
                }

                if (b != newImage[i + 2])
                {
                    derivationImage[i + 2] = 255;
                    anyDerivation = true;
                    differentComponents++;
                }

                if (a != newImage[i + 3])
                {
                    derivationImage[i + 3] = 255;
                    anyDerivation = true;
                    differentComponents++;
                }

                // If any derivation set the alpha of the derivation image to max.
                if (anyDerivation)
                {
                    derivationImage[i + 3] = 255;
                }
                // Check if the pixel is empty, in which case we don't want it to count to the total.
                else
                {
                    if (r == 0 && g == 0 && b == 0 && a == 0
                        && newImage[i] == 0 && newImage[i + 1] == 0 && newImage[i + 2] == 0 && newImage[i + 3] == 0)
                    {
                        emptyPixels++;
                    }
                }
            }

            int totalComponents = originalImage.Length - emptyPixels * 4;
            if (totalComponents == 0) return 0;

            float derivation = (float) differentComponents / totalComponents;
            return derivation * 4;
        }

        #endregion

        #region Performance Comparison

        /// <summary>
        /// Test the performance of an action call.
        /// </summary>
        /// <param name="iterations">The times to iterate.</param>
        /// <param name="act">The action itself.</param>
        /// <param name="inMs">Whether to return the result in milliseconds instead of ticks.</param>
        /// <returns>The time it took in ticks.</returns>
        public static long PerformanceTest(int iterations, Action act, bool inMs = false)
        {
            var timer = new Stopwatch();
            var results = new long[iterations];

            for (var i = 0; i < iterations; i++)
            {
                timer.Restart();

                act();

                results[i] = inMs ? timer.ElapsedMilliseconds : timer.ElapsedTicks;
            }

            timer.Stop();
            return results.Sum() / results.Length;
        }

        /// <summary>
        /// Test the performance of an action call, with an additional helper call.
        /// </summary>
        /// <param name="iterations">The times to iterate.</param>
        /// <param name="helperAct">The action to call each time, but not measure.</param>
        /// <param name="act">The action itself.</param>
        /// <param name="inMs">Whether to return the result in milliseconds instead of ticks.</param>
        /// <returns>The time it took in ticks.</returns>
        public static long PerformanceTest(int iterations, Action helperAct, Action act, bool inMs = false)
        {
            var timer = new Stopwatch();
            var results = new long[iterations];

            for (var i = 0; i < iterations; i++)
            {
                helperAct();

                timer.Restart();

                act();

                results[i] = inMs ? timer.ElapsedMilliseconds : timer.ElapsedTicks;
            }

            timer.Stop();
            return results.Sum() / results.Length;
        }

        #endregion
    }
}