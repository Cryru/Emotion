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
using Emotion.Common.Threading;
using Emotion.Graphics.Objects;
using Emotion.Standard.Image.PNG;
using Emotion.Standard.Logging;
using Emotion.Test.Helpers;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Test
{
    public static class Runner
    {
        #region Settings

        /// <summary>
        /// Whether to not run linked runners.
        /// </summary>
        public static bool NoLinkedRunners;

        /// <summary>
        /// What percentage of the whole image's pixels can be different.
        /// </summary>
        public static float PixelDerivationTolerance = 10;

        /// <summary>
        /// The folder when rendered results are stored.
        /// </summary>
        public static string RenderResultStorage = "ReferenceImages";

        #endregion

        #region Configuration Holders

        /// <summary>
        /// Other runners to run. and the arguments to run them with.
        /// testOnly - Means that the engine will not be initialized. Used for running unit tests which don't depend on the engine.
        /// tag - Means that only tests with this tag will be run. If the tests are set to "tagOnly" that means that they will be
        /// run only if filtered by tag.
        /// The function argument can be used to specify a different engine config.
        /// </summary>
        private static Dictionary<string, Action<Configurator>> _otherConfigs;

        /// <summary>
        /// Screenshot db.
        /// </summary>
        private static Dictionary<string, byte[]> _screenResultDb;

        #endregion

        /// <summary>
        /// The id of the runner instance.
        /// </summary>
        public static int RunnerId { get; private set; } = Process.GetCurrentProcess().Id;

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

        private static Action<float> _loopAction;
        private static ManualResetEvent _loopWaiter;
        private static int _loopCounter;

        private static List<LinkedRunner> _linkedRunners = new List<LinkedRunner>();
        private static Regex _testCompletedRegex = new Regex(@"(?:Test completed: )([0-9]+?)\/([0-9]*)(?:!)");

        private static bool _runnerFolderCreated;

        private static Exception _loopException;

        private static Dictionary<string, int> _comparisonImageDuplicate = new Dictionary<string, int>();

        private static object _threadLock = new object();

        /// <summary>
        /// Run tests.
        /// </summary>
        /// <param name="engineConfig">The default engine config. All configs in "otherConfigs" are modifications of this one.</param>
        /// <param name="args">The execution args passed to the Main. This is needed to coordinate linked runners.</param>
        /// <param name="otherConfigs">List of engine configurations to spawn runners with.</param>
        /// <param name="screenResultDb">Database of screenshot results to compare against when using VerifyImage</param>
        public static void RunTests(Configurator engineConfig, string[] args = null, Dictionary<string, Action<Configurator>> otherConfigs = null, Dictionary<string, byte[]> screenResultDb = null)
        {
            if (args == null) args = new string[] { };
            _otherConfigs = otherConfigs ?? new Dictionary<string, Action<Configurator>>();
            _screenResultDb = screenResultDb ?? new Dictionary<string, byte[]>();

            // Check for test run id. This signifies whether the runner is linked.
            TestRunId = CommandLineParser.FindArgument(args, "testRunId=", out string testRunId) ? testRunId : RunnerId.ToString();
            TestRunFolder = Path.Join("TestResults", $"{TestRunId}");

            CommandLineParser.FindArgument(args, "tag=", out string tag);
            RunnerReferenceImageFolder = Path.Join(TestRunFolder, RenderResultStorage, $"LR{RunnerId}References{tag}");

            // Check if master runner.
            bool linked = TestRunId != RunnerId.ToString();
            LoggingProvider log = new TestRunnerLogger(RunnerId.ToString(), linked, Path.Join(TestRunFolder, "Logs"));

            // Set the default engine settings for the test runner.
            Configurator config = engineConfig;
            config.DebugMode = true;
            config.LoopFactory = TestLoop;
            config.Logger = log;

            // Check if a custom engine config is to be loaded. This check is a bit elaborate since the config params are merged with the linked params.
            string argsJoined = string.Join(" ", args);
            string id = (from possibleConfigs in _otherConfigs where argsJoined.Contains(possibleConfigs.Key) select possibleConfigs.Key).FirstOrDefault();
            if (id != null && _otherConfigs.ContainsKey(id) && _otherConfigs[id] != null)
            {
                Engine.Log.Info($"Loading custom engine config - {id}...", CustomMSource.TestRunner);
                _otherConfigs[id](config);
            }

            // Perform light setup.
            Engine.LightSetup(config);

            // Run linked runners (if the master).
            if (linked)
            {
                log.Info($"I am a linked runner with arguments {string.Join(" ", args)}", CustomMSource.TestRunner);
            }
            else
            {
                // Spawn linked runners
                if (!NoLinkedRunners)
                    // Spawn a runner for each runtime config.
                    foreach ((string arg, Action<Configurator> _) in _otherConfigs)
                    {
                        _linkedRunners.Add(new LinkedRunner(arg));
                    }
            }

            // Check if running only specific tests.
            if (CommandLineParser.FindArgument(args, "tag=", out string testTag)) TestTag = testTag;

            // Check if running tests without an engine instance - this shouldn't be used with a tag because most tests except an instance.
            if (CommandLineParser.FindArgument(args, "testOnly", out string _))
            {
                Task tests = Task.Run(BeginRun);
                while (!tests.IsCompleted) TestLoopUpdate();
                Engine.Quit();
                return;
            }

            // Perform engine setup.
            Engine.Setup(config);

            // Move the camera center in a way that its center is 0,0
            Engine.Renderer.Camera.Position += new Vector3(Engine.Renderer.Camera.WorldToScreen(Vector2.Zero), 0);
            Task.Run(() =>
            {
                // Wait for the engine to start.
                while (Engine.Status != EngineStatus.Running)
                {
                }

                // If crashed.
                if (Engine.Status == EngineStatus.Stopped) return;

                // Name the thread.
                if (Engine.Host?.NamedThreads ?? false) Thread.CurrentThread.Name ??= "Runner Thread";

                BeginRun();

                Engine.Quit();

                // Wait for the engine to stop.
                while (Engine.Status == EngineStatus.Running)
                {
                }
            });
            Engine.Run();
        }

        private static void BeginRun()
        {
            // Find all test classes.
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null) return;
            Type[] testClasses = entryAssembly.GetTypes().AsParallel().Where(x => x.GetCustomAttributes(typeof(TestAttribute), true).Length > 0).ToArray();

            // Find all test functions in these classes.
            var tests = new List<MethodInfo>();
            foreach (Type classType in testClasses)
            {
                // Check if filtering by tag.
                var t = (TestAttribute) classType.GetCustomAttributes(typeof(TestAttribute), true).FirstOrDefault();
                if (!string.IsNullOrEmpty(TestTag) && t?.Tag != TestTag)
                {
#if TEST_DEBUG
                    Log.Trace($"Skipping class {classType} because it doesn't match tag filter '{TestTag}'.", CustomMSource.TestRunner);
#endif
                    continue;
                }

                if (string.IsNullOrEmpty(TestTag) && (t?.TagOnly ?? false))
                {
#if TEST_DEBUG
                    Log.Trace($"Skipping class {classType} because it will only run with the tag '{t.Tag}'.", CustomMSource.TestRunner);
#endif
                    continue;
                }

                // Find all test functions in this class.
                tests.AddRange(classType.GetMethods().AsParallel().Where(x => x.GetCustomAttributes(typeof(TestAttribute), true).Length > 0).ToArray());
            }

            Type currentClass = null;
            object currentClassInstance = null;
            float classTimer = 0;

            var timeTracker = new Stopwatch();
            var failedTests = 0;

            foreach (MethodInfo func in tests)
            {
                // Create an instance of the test class.
                if (currentClass != func.DeclaringType)
                {
                    if (currentClass != null) Engine.Log.Info($"Test class {currentClass} completed in {classTimer}ms!", CustomMSource.TestRunner);
                    currentClass = func.DeclaringType;
                    if (currentClass == null) throw new Exception($"Declaring type of function {func.Name} is missing.");
                    currentClassInstance = Activator.CreateInstance(currentClass);
                    classTimer = 0;
                    Engine.Log.Info($"Running test class {currentClass}...", CustomMSource.TestRunner);
                }

                // Run test.
                Engine.Log.Info($"  Running test {func.Name}...", CustomMSource.TestRunner);
                timeTracker.Restart();
#if !THROW_EXCEPTIONS
                try
                {
#endif
                    func.Invoke(currentClassInstance, new object[] { });

                    // Check if errored in the loop.
                    if (_loopException != null)
                    {
                        var wrapped = new Exception("Exception in test engine loop.", _loopException);
                        _loopException = null;
                        throw wrapped;
                    }
#if !THROW_EXCEPTIONS
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
                        Engine.Log.Error($"{ex.InnerException.Message}", CustomMSource.TestRunner);
                        continue;
                    }

                    Engine.Log.Error($" Test {func.Name} failed - {ex}", CustomMSource.TestRunner);
                    Debug.Assert(false);
                }
#endif

                Engine.Log.Info($"  Test {func.Name} completed in {timeTracker.ElapsedMilliseconds}ms!", CustomMSource.TestRunner);
                classTimer += timeTracker.ElapsedMilliseconds;
            }

            Engine.Log.Info($"Test completed: {tests.Count - failedTests}/{tests.Count}!", CustomMSource.TestRunner);

            // If not the master - then nothing else to do.
            if (TestRunId != RunnerId.ToString()) return;

            var results = new List<string> {$"Master: Test completed: {tests.Count - failedTests}/{tests.Count}!"};
            int totalTests = tests.Count;
            var error = false;

            // Wait for linked runners to exit.
            foreach (LinkedRunner linked in _linkedRunners)
            {
                Engine.Log.Info("----------------------------------------------------------------------", CustomMSource.TestRunner);
                Engine.Log.Info($"Waiting for LR{linked.Id} - ({linked.Args})", CustomMSource.TestRunner);
                int exitCode = linked.WaitForFinish(out string output, out string errorOutput);
                output = output.Trim();
                errorOutput = errorOutput.Trim();

                // Try to find the test completed line.
                Match match = _testCompletedRegex.Match(output);
                var result = "";
                if (match.Success)
                    try
                    {
                        int testsSuccess = int.Parse(match.Groups[1].Value);
                        int testsRun = int.Parse(match.Groups[2].Value);

                        int failed = testsRun - testsSuccess;
                        failedTests += failed;
                        totalTests += testsRun;

                        result = match.Groups[0].Value;
                    }
                    catch (Exception)
                    {
                        Engine.Log.Info($"Couldn't read tests completed from LR{linked.Id}.", CustomMSource.TestRunner);
                    }
                else
                    result = "<Unknown>/<Unknown>";

                var anyError = "";
                if (!string.IsNullOrEmpty(errorOutput))
                {
                    error = true;
                    anyError = "ERR ";
                }

                results.Add($"LR{linked.Id} ({linked.Args}) {result} {anyError}{linked.TimeElapsed}ms");

                Engine.Log.Info($"LR{linked.Id} exited with code {exitCode}.", CustomMSource.TestRunner);
                Engine.Log.Info($"Dumping log from LR{linked.Id}\n{output}", CustomMSource.TestRunner);
                if (!string.IsNullOrEmpty(errorOutput)) Engine.Log.Info($"[LR{linked.Id}] Error Output\n{errorOutput}", CustomMSource.TestRunner);
            }

            // Post final results.
            Engine.Log.Info($"Final test results: {totalTests - failedTests}/{totalTests} {(error ? "Errors found!" : "")}!", CustomMSource.TestRunner);
            foreach (string r in results)
            {
                Engine.Log.Info($"     {r}", CustomMSource.TestRunner);
            }

            if (error || failedTests > 0) Environment.Exit(1);
        }

        #region Loop

        private static void TestLoop(Action tick, Action frame)
        {
            while (Engine.Status == EngineStatus.Running)
            {
                if (!Engine.Host.Update()) break;
                Engine.DeltaTime = 0;

                GLThread.Run();
                bool run = TestLoopUpdate();

                if (!run) continue;
                Engine.DeltaTime = 16;
                tick();
                frame();
            }

            Engine.Quit();
        }

        private static bool TestLoopUpdate()
        {
#if !THROW_EXCEPTIONS
            try
            {
#endif
                // Check running loops.
                if (_loopAction == null) return true;

#if !THROW_EXCEPTIONS
                try
                {
#endif
                    _loopAction.Invoke(Engine.DeltaTime);
#if !THROW_EXCEPTIONS
                }
                catch (Exception ex)
                {
                    _loopException = ex;
                }
#endif

                _loopCounter--;
                int counterThisTick = _loopCounter;

                if (counterThisTick > 0) return false;
                if (counterThisTick == 0) _loopAction = null;

                // Release the lock.
                lock (_threadLock)
                {
                    _loopWaiter?.Set();
                    if (counterThisTick == 0) _loopWaiter = null;
                }

                return true;
#if !THROW_EXCEPTIONS
            }
            catch (Exception ex)
            {
                Engine.Log.Error($"Test runner encountered error in loop - {ex}", MessageSource.Other);
                Debug.Assert(false);
                return false;
            }
#endif
        }

        /// <summary>
        /// Execute the action as if it was in the main loop.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="times">The number of times to execute.</param>
        /// <returns>A token to await the execution on.</returns>
        public static ManualResetEvent ExecuteAsLoop(Action<float> action, int times = 1)
        {
            lock (_threadLock)
            {
                Debug.Assert(_loopWaiter == null);
                _loopAction = action;
                _loopWaiter = new ManualResetEvent(false);
                _loopCounter = times;
            }

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
            ExecuteAsLoop(f => { }, count).WaitOne();
        }

        /// <summary>
        /// Used for debugging a specific linked runner.
        /// </summary>
        // ReSharper disable once RedundantAssignment
        public static void RunAsRunner(string text, ref string[] args)
        {
            NoLinkedRunners = true;
            args = text.Split(" ");
        }

        #endregion

        #region Image Comparison

        /// <summary>
        /// Verify what has been rendered with a predefined render result.
        /// </summary>
        /// <param name="renderId">The cached id to check against.</param>
        public static void VerifyScreenshot(string renderId)
        {
            VerifyScreenshot(renderId, Engine.Renderer.DrawBuffer);
        }

        public static void VerifyScreenshot(string renderId, FrameBuffer b)
        {
            // Take a screenshot to compare to the expected image. Assume the sizes are the same.
            byte[] screenshot = b.Sample(b.Viewport, PixelFormat.Rgba);
            Vector2 screenShotSize = b.Size;
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
            if (_screenResultDb.ContainsKey(renderId))
                cachedRender = _screenResultDb[renderId];
            else
                Engine.Log.Warning($"      Missing comparison render with id {renderId}.", CustomMSource.TestRunner);

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
            if (!_runnerFolderCreated)
            {
                Directory.CreateDirectory(RunnerReferenceImageFolder);
                _runnerFolderCreated = true;
            }

            // Invent a name for this comparison and a folder to store data in, in case it is derived.
            string fileName;
            if (_comparisonImageDuplicate.ContainsKey(compareName))
            {
                fileName = $"{compareName}{_comparisonImageDuplicate[compareName]}.png";
                _comparisonImageDuplicate[compareName]++;
            }
            else
            {
                fileName = $"{compareName}.png";
                _comparisonImageDuplicate.Add(compareName, 1);
            }

            // We want to store the comparison image for possible manual comparison.
            SaveReferenceImage(fileName, comparisonSize, comparisonImage);

            // Check if the original image is missing, in which case we just store the comparison image.
            if (originalImage == null) return;

            Engine.Log.Info($"      Comparing images {compareName}...", CustomMSource.TestRunner);

            float derivedPixelPercentage;
            byte[] derivationImage = null;

            // If the size isn't the same, it is all derived.
            if (originalImage.Length == comparisonImage.Length)
                derivedPixelPercentage = CalculateImageDerivation(originalImage, comparisonImage, out derivationImage) * 100;
            else
                derivedPixelPercentage = 100;

            if (derivedPixelPercentage == 0)
            {
                Engine.Log.Info("          No derivation.", CustomMSource.TestRunner);
                return;
            }

            // Save a derivation image showing the differences.
            if (derivationImage != null)
            {
                string directory = Path.Join(RunnerReferenceImageFolder, $"Comparison_{fileName}");
                Directory.CreateDirectory(directory);
                byte[] derivedFile = PngFormat.Encode(ImageUtil.FlipImageYNoMutate(derivationImage, (int) comparisonSize.Y), comparisonSize, PixelFormat.Rgba);
                File.WriteAllBytes(Path.Join(directory, "derivation.png"), derivedFile);
            }

            // Assert derivation is not higher than tolerable. This is not done using the Emotion.Test assert so it doesn't stop the test from continuing.
            if (derivedPixelPercentage > PixelDerivationTolerance) throw new ImageDerivationException($"          Failed derivation check. Derivation is {derivedPixelPercentage}%.");
            Engine.Log.Info($"          Derivation is {derivedPixelPercentage}%.", CustomMSource.TestRunner);
        }

        /// <summary>
        /// Save an image for reference.
        /// </summary>
        /// <param name="fileName">The image name.</param>
        /// <param name="size">The image size.</param>
        /// <param name="pixels">The image pixels.</param>
        public static void SaveReferenceImage(string fileName, Vector2 size, byte[] pixels)
        {
            if (!_runnerFolderCreated)
            {
                Directory.CreateDirectory(RunnerReferenceImageFolder);
                _runnerFolderCreated = true;
            }

            string filePath = Path.Join(RunnerReferenceImageFolder, fileName);
            byte[] file = PngFormat.Encode(ImageUtil.FlipImageYNoMutate(pixels, (int) size.Y), size, PixelFormat.Rgba);
            File.WriteAllBytes(filePath, file);
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
                        emptyPixels++;
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