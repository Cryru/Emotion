#nullable enable

#region Using

using Emotion.Game.Time.Routines;
using Emotion.Platform.Implementation.Win32;
using Emotion.Utility;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

#endregion

namespace Emotion.Testing;

public class TestExecutionReport
{
    public int Completed;
    public int Total;
}

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
    public static float PixelDerivationTolerance = 2;

    private static Type? _testsFilter;

    public static void ExecuteTests(string[] args, Configurator? config = null, Type? filterTestsOnlyFromClass = null)
    {
        // todo: read args and start running split processes, different configs etc.

        string resultFolder = CommandLineParser.FindArgument(args, "folder=", out string folderPassed) ? folderPassed : $"{DateTime.Now:MM-dd-yyyy(HH.mm.ss)}";
        TestRunFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TestResults", resultFolder);

        config ??= new Configurator();
        config.DebugMode = true;
        config.NoErrorPopup = true;
        config.Logger = new TestLogger(Path.Join(TestRunFolder, "Logs"));

        _testsFilter = filterTestsOnlyFromClass;

        Engine.Start(config, TestSystemInitRoutineAsync);
    }

    private static IEnumerator TestSystemInitRoutineAsync()
    {
        Coroutine testRoutine = Engine.CoroutineManagerAsync.StartCoroutine(RunTestsRoutineAsync());
        yield return testRoutine;
        Engine.Quit();
    }

    private static IEnumerator RunTestsRoutineAsync()
    {
        // Find all test functions in test classes.
        var testFunctions = new List<MethodInfo>();
        var testFunctionsDebugOnly = new List<MethodInfo>();
        foreach (Assembly ass in Helpers.AssociatedAssemblies)
        {
            IEnumerable<Type> classTypes = ass.GetTypes().Where(x => x.GetCustomAttributes(typeof(TestAttribute), true).Length > 0);
            foreach (Type classType in classTypes)
            {
                if (classType.IsSubclassOf(typeof(TestingScene)))
                {
                    Engine.Log.Warning($"Scene {classType.Name} doesn't need a test attribute since it inherits TestScene", MessageSource.Test);
                    continue;
                }

                if (_testsFilter != null && classType != _testsFilter) continue;

                // Get all test functions in this class.
                IEnumerable<MethodInfo> methodsInTestClass = classType.GetMethods().Where(x => x.GetCustomAttributes(typeof(TestAttribute), true).Length > 0);

                testFunctionsDebugOnly.AddRange(GetFunctionsWithDebugThis(classType, methodsInTestClass));
                testFunctions.AddRange(methodsInTestClass);
            }
        }

        // Find all test scenes.
        var testScenes = new List<Type>();
        var testScenesWithMethods = new Dictionary<Type, List<MethodInfo>>();
        var testScenesWithMethodsDebugThis = new Dictionary<Type, List<MethodInfo>>();
        foreach (Assembly ass in Helpers.AssociatedAssemblies)
        {
            IEnumerable<Type> sceneTypes = ass.GetTypes().Where(x => x.IsSubclassOf(typeof(TestingScene)));
            foreach (Type sceneType in sceneTypes)
            {
                if (_testsFilter != null && sceneType != _testsFilter) continue;

                List<MethodInfo> methodsInThisClass = new();
                List<MethodInfo> methodsInThisClassDebugThis = new();

                // Get all test functions
                IEnumerable<MethodInfo> methodsInTestScene = sceneType.GetMethods().Where(x => x.GetCustomAttributes(typeof(TestAttribute), true).Length > 0);

                methodsInThisClassDebugThis.AddRange(GetFunctionsWithDebugThis(sceneType, methodsInTestScene));
                if (methodsInThisClassDebugThis.Count > 0)
                    testScenesWithMethodsDebugThis.Add(sceneType, methodsInThisClassDebugThis);

                methodsInThisClass.AddRange(methodsInTestScene);
                if (methodsInThisClass.Count > 0)
                    testScenesWithMethods.Add(sceneType, methodsInThisClass);
            }
        }

#if !AUTOBUILD
        // Debugging tests!
        if (testFunctionsDebugOnly.Count > 0 || testScenesWithMethodsDebugThis.Count > 0)
        {
            testFunctions = testFunctionsDebugOnly;
            testScenesWithMethods = testScenesWithMethodsDebugThis;
        }
#endif

        var reportClasses = new TestExecutionReport();
        yield return RunTestClasses(testFunctions, reportClasses);

        var reportScenes = new TestExecutionReport();
        yield return RunTestScenesRoutineAsync(testScenesWithMethods, reportScenes);

        // The format of this message must match the old system's regex!
        var completed = reportClasses.Completed + reportScenes.Completed;
        var total = reportClasses.Total + reportScenes.Total;
        Engine.Log.Info($"Test completed: {completed}/{total}!", MessageSource.Test);

#if !AUTOBUILD
        if (Engine.Host is Win32Platform win32) win32.OpenFolderAndSelectFile(TestRunFolder + "\\");
#endif
    }

    private static IEnumerator RunTestClasses(List<MethodInfo> testFunctions, TestExecutionReport report)
    {
        Stopwatch timer = new Stopwatch();

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

            bool hasCoroutines = false;
            for (int i = 0; i < functions.Count; i++)
            {
                var func = functions[i];
                hasCoroutines = func.ReturnType.IsAssignableTo(typeof(IEnumerator));
                if (hasCoroutines) break;
            }

            if (declaringType.GetCustomAttribute<TestClassRunParallel>() != null && !hasCoroutines)
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
                    _currentSceneCurrentRoutineFailed = false;

                    MethodInfo func = testClass.Value[i];
                    Coroutine coroutine = Coroutine.CompletedRoutine; // In case function is a routine

                    // Run test function in try-catch.
                    timer.Start();
                    Engine.Log.Info($"  Running test {func.Name}...", MessageSource.Test);

                    object? returnVal = null;
                    try
                    {
                        returnVal = func.Invoke(currentClassInstance, new object[] { });
                    }
                    catch (Exception)
                    {
                        // ignored, it's printed by the internal engine error handling
                        _currentSceneCurrentRoutineFailed = true;
                    }

                    // Function is actually a routine function
                    if (returnVal != null && returnVal is IEnumerator routineFunc)
                    {
                        coroutine = Engine.CoroutineManager.StartCoroutine(routineFunc);
                        Engine.Log.Info($"    Running coroutine", MessageSource.Test);
                    }
                    yield return coroutine;

                    if (coroutine.Stopped) _currentSceneCurrentRoutineFailed = true;
                    if (!_currentSceneCurrentRoutineFailed)
                    {
                        completed++;
                        completedThisClass++;
                    }

                    Engine.Log.Info($"    Elapsed: {timer.ElapsedMilliseconds}ms", MessageSource.Test);
                    timer.Stop();
                }
            }

            Engine.Log.Info($"Completed {declaringType}: {completedThisClass}/{totalThisClass}!\n", MessageSource.Test);
        }

        report.Completed = completed;
        report.Total = total;
    }

    private static IEnumerator RunTestScenesRoutineAsync(Dictionary<Type, List<MethodInfo>> testScenes, TestExecutionReport report)
    {
        int completed = 0;
        int total = 0;

        foreach (KeyValuePair<Type, List<MethodInfo>> scenePair in testScenes)
        {
            Type sceneType = scenePair.Key;
            object? sceneInstanceObject = Activator.CreateInstance(sceneType);
            if (sceneInstanceObject is not TestingScene testScene)
            {
                Engine.Log.Error($"Couldn't initialize test scene of type {sceneType.Name}", MessageSource.Test);
                continue;
            }

            Engine.Log.Info($"\nRunning test scene {sceneType}...", MessageSource.Test); // \n to add empty line
            yield return Engine.SceneManager.SetScene(testScene);
            TestingScene.SetCurrent(testScene);

            List<MethodInfo> functions = scenePair.Value;
            int totalThisScene = functions.Count;
            var completedThisScene = 0;
            total += totalThisScene;

            foreach (MethodInfo testFunction in functions)
            {
                string functionName = testFunction.Name;
                Engine.Log.Info($"  Running test {functionName}...", MessageSource.Test);

                _currentSceneCurrentRoutineFailed = false;

                Coroutine coroutine = Coroutine.CompletedRoutine;

                testScene.BetweenEachTest();

                // Run the function
                object? returnVal = testFunction.Invoke(testScene, null);
                if (returnVal is IEnumerator routineFunc)
                {
                    coroutine = Engine.CoroutineManager.StartCoroutine(routineFunc);
                    Engine.Log.Info($"    Running coroutine", MessageSource.Test);
                }

                yield return coroutine;

                if (coroutine.Stopped) _currentSceneCurrentRoutineFailed = true;
                if (!_currentSceneCurrentRoutineFailed)
                {
                    completedThisScene++;
                    completed++;
                }
            }

            TestingScene.SetCurrent(null);
            Engine.Log.Info($"Completed {sceneType}: {completedThisScene}/{totalThisScene}!\n", MessageSource.Test);
        }

        report.Completed = completed;
        report.Total = total;
    }

    private static bool _currentSceneCurrentRoutineFailed = false;

    public static void SetCurrentTestSceneTestAsFailed()
    {
        _currentSceneCurrentRoutineFailed = true;
    }

    private static IEnumerable<MethodInfo> GetFunctionsWithDebugThis(Type parentType, IEnumerable<MethodInfo> methodsInTestClass)
    {
        bool classIsDebugThis = parentType.GetCustomAttribute<DebugTestAttribute>() != null;
        if (classIsDebugThis) return methodsInTestClass;

        return methodsInTestClass.Where(x => x.GetCustomAttributes(typeof(DebugTestAttribute), true).Length > 0);
    }
}