#nullable enable

#region Using

using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Game.Time.Routines;
using Emotion.Platform.Implementation.Win32;
using Emotion.Utility;

#endregion

namespace Emotion.Testing;

public static class TestExecutor
{
    /// <summary>
    /// Whether we allow tests to halt with an infinite game loop waiter.
    /// Disable in CI and such.
    /// </summary>
#if AUTOBUILD
	public static bool AllowInfiniteLoops = false;
#else
    public static bool AllowInfiniteLoops = true;
#endif

    /// <summary>
    /// The folder this test run will store output in, such as logs and
    /// reference renders.
    /// </summary>
    public static string TestRunFolder = "";

    /// <summary>
    /// What percentage of the whole image's pixels can be different.
    /// </summary>
    public static float PixelDerivationTolerance = 10;

    public static void ExecuteTests(string[] args, Configurator? config = null, Type? filterTestsOnlyFromClass = null)
    {
        // todo: read args and start running split processes, different configs etc.

        string resultFolder = CommandLineParser.FindArgument(args, "folder=", out string folderPassed) ? folderPassed : $"{DateTime.Now:MM-dd-yyyy(HH.mm.ss)}";

        TestRunFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TestResults", resultFolder);

        config ??= new Configurator();
        config.DebugMode = true;
        config.NoErrorPopup = true;
        config.Logger = new TestLogger(Path.Join(TestRunFolder, "Logs"));

        Engine.Setup(config);

        // Find all test functions in test classes.
        var testFunctions = new List<MethodInfo>();
        foreach (Assembly ass in Helpers.AssociatedAssemblies)
        {
            IEnumerable<Type> classTypes = ass.GetTypes().Where(x => x.GetCustomAttributes(typeof(TestAttribute), true).Length > 0);
            foreach (Type classType in classTypes)
            {
                if (filterTestsOnlyFromClass != null && classType != filterTestsOnlyFromClass) continue;

                // Find all test functions in this class.
                testFunctions.AddRange(classType.GetMethods().Where(x => x.GetCustomAttributes(typeof(TestAttribute), true).Length > 0));
            }
        }

        // Find all test scenes.
        var testScenes = new List<Type>();
        foreach (Assembly ass in Helpers.AssociatedAssemblies)
        {
            IEnumerable<Type> sceneTypes = ass.GetTypes().Where(x => x.IsSubclassOf(typeof(TestingScene)));
            foreach (Type sceneType in sceneTypes)
            {
                if (filterTestsOnlyFromClass != null && sceneType != filterTestsOnlyFromClass) continue;

                testScenes.Add(sceneType);
            }
        }

        Task.Run(async () =>
        {
            try
            {
                (int completed, int total) resultClasses = RunTestClasses(testFunctions);
                (int completed, int total) resultScenes = await RunTestScenes(testScenes);

                // The format of this message must match the old system's regex!
                var completed = resultClasses.completed + resultScenes.completed;
                var total = resultClasses.total + resultScenes.total;
                Engine.Log.Info($"Test completed: {completed}/{total}!", MessageSource.Test);

#if !AUTOBUILD
                if (Engine.Host is Win32Platform win32) win32.OpenFolderAndSelectFile(TestRunFolder + "\\");
#endif
            }
            catch (Exception)
            {
                // ignored, prevent stalling
            }

            Engine.Quit();
        });

        Engine.Run();
    }

    private static (int completed, int total) RunTestClasses(List<MethodInfo> testFunctions)
    {
        int completed = 0;
        int total = 0;

        Dictionary<Type, List<MethodInfo>> testsByClass = new();
        List<MethodInfo>? currentClassList = null;
        Type? currentClass = null;
        foreach (MethodInfo func in testFunctions)
        {
            // Create an instance of the test class.
            if (currentClass != func.DeclaringType)
            {
                if (currentClassList != null && currentClass != null) testsByClass.Add(currentClass, currentClassList);
                currentClassList = new List<MethodInfo>();
                currentClass = func.DeclaringType;
            }

            currentClassList?.Add(func);
        }

        if (currentClassList != null && currentClass != null) testsByClass.Add(currentClass, currentClassList);

        foreach (KeyValuePair<Type, List<MethodInfo>> testClass in testsByClass)
        {
            List<MethodInfo> functions = testClass.Value;
            total += functions.Count;

            var completedThisClass = 0;
            var totalThisClass = functions.Count;

            Type declaringType = testClass.Key;
            Engine.Log.Info($"\nRunning test class {declaringType}...", MessageSource.Test);
            object? currentClassInstance = Activator.CreateInstance(declaringType);

            if (declaringType.GetCustomAttribute<TestClassRunParallel>() != null)
            {
                Engine.Log.Info("=-= Parallel Execution =-=", MessageSource.Test);

                var tasks = new Task[functions.Count];
                for (var i = 0; i < functions.Count; i++)
                {
                    MethodInfo func = functions[i];
#if true
                    tasks[i] = Task.Run(() =>
                    {
                        try
                        {
                            Engine.Log.Info($"  Running test {func.Name}...", MessageSource.Test);
                            func.Invoke(currentClassInstance, new object[] { });
                            Interlocked.Add(ref completed, 1);
                            Interlocked.Add(ref completedThisClass, 1);
                        }
                        catch (Exception)
                        {
                            // ignored, it's printed by the internal engine error handling
                        }
                    });
#else
                    Engine.Log.Info($"  Running test {func.Name}...", MessageSource.Test);
                    func.Invoke(currentClassInstance, new object[] { });
                    completedInTestClasses++;
                    completedThisClass++;
#endif
                }

                Task.WaitAll(tasks, 30_000);
            }
            else
            {
                for (var i = 0; i < testClass.Value.Count; i++)
                {
                    MethodInfo func = testClass.Value[i];
                    try
                    {
                        // Run test.
                        Engine.Log.Info($"  Running test {func.Name}...", MessageSource.Test);
                        func.Invoke(currentClassInstance, new object[] { });
                        completed++;
                        completedThisClass++;
                    }
                    catch (Exception)
                    {
                        // ignored, it's printed by the internal engine error handling
                    }
                }
            }

            Engine.Log.Info($"Completed {declaringType}: {completedThisClass}/{totalThisClass}!\n", MessageSource.Test);
        }

        return (completed, total);
    }

    private static async Task<(int, int)> RunTestScenes(List<Type> testScenes)
    {
        int completed = 0;
        int total = 0;

        for (var i = 0; i < testScenes.Count; i++)
        {
            Type sceneType = testScenes[i];
            object? sceneInstance = Activator.CreateInstance(sceneType);
            if (sceneInstance is not TestingScene sc)
            {
                Engine.Log.Error($"Couldn't initialize test scene of type {sceneType.Name}", MessageSource.Test);
                continue;
            }

            var completedThisScene = 0;
            var totalThisScene = 0;

            Engine.Log.Info($"\nRunning test scene {sceneType}...", MessageSource.Test);
            await Engine.SceneManager.SetScene(sc);
            Func<IEnumerator>[] testRoutines = sc.GetTestCoroutines();
            foreach (Func<IEnumerator> testRoutine in testRoutines)
            {
                MethodInfo routineReflect = testRoutine.GetMethodInfo();
                string functionName = routineReflect.Name;
                Engine.Log.Info($"  Running test {functionName}...", MessageSource.Test);

                bool testFailed = false;

                sc.RunningTestRoutineIndex++;
                IEnumerator enumerator = testRoutine();
                Coroutine coroutine = Engine.CoroutineManager.StartCoroutine(enumerator);
                while (!coroutine.Finished && !coroutine.Stopped)
                {
                    IRoutineWaiter? currentWaiter = coroutine.CurrentWaiter;
                    while (currentWaiter is Coroutine subRoutine) currentWaiter = subRoutine.CurrentWaiter;

                    if (currentWaiter is VerifyScreenshotResult verifyResult)
                        if (!verifyResult.Passed) testFailed = true;

                    if (currentWaiter is TestWaiterRunLoops runLoopsWaiter && !runLoopsWaiter.Finished)
                    {
                        if (runLoopsWaiter.LoopsToRun == -1)
                        {
                            sc.RunLoopsConstant(true);
                        }
                        else
                        {
                            sc.RunLoop();
                            runLoopsWaiter.AddLoopRan();
                        }
                    }
                }

                totalThisScene++;
                total++;

                if (coroutine.Stopped) testFailed = true;
                if (!testFailed)
                {
                    completedThisScene++;
                    completed++;
                } 
            }

            Engine.Log.Info($"Completed {sceneType}: {completedThisScene}/{totalThisScene}!\n", MessageSource.Test);
        }

        return (completed, total);
    }
}