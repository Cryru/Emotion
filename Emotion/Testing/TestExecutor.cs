#region Using

using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Emotion.Game.Time.Routines;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Testing;

public static class TestExecutor
{
	/// <summary>
	/// Whether we allow tests to halt with an infinite game loop waiter.
	/// Disable in CI and such.
	/// </summary>
	public static bool AllowInfiniteLoops = true;

	/// <summary>
	/// The folder this test run will store output in, such as logs and
	/// reference renders.
	/// </summary>
	public static string TestRunFolder = "";

	/// <summary>
	/// The folder this test run will store reference renders in.
	/// </summary>
	public static string ReferenceImageFolder = "";

	public static void ExecuteTests(string[] args, Configurator? config = null)
	{
		// todo: read args and start running split processes, different configs etc.

		string resultFolder = CommandLineParser.FindArgument(args, "folder=", out string folderPassed) ? folderPassed : $"{DateTime.Now:MM-dd-yyyy(HH.mm.ss)}";

		TestRunFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TestResults", resultFolder);
		ReferenceImageFolder = Path.Join(TestRunFolder, "ReferenceImages");

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
				testScenes.Add(sceneType);
			}
		}

		Task.Run(async () =>
		{
			try
			{
				RunTestClasses(testFunctions);
				await RunTestScenes(testScenes);
			}
			catch (Exception)
			{
				// ignored, prevent stalling
			}

			Engine.Quit();
		});

		Engine.Run();
	}

	private static void RunTestClasses(List<MethodInfo> testFunctions)
	{
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

		var completed = 0;
		var total = 0;

		foreach (KeyValuePair<Type, List<MethodInfo>> testClass in testsByClass)
		{
			List<MethodInfo> functions = testClass.Value;
			total += functions.Count;

			Type declaringType = testClass.Key;
			Engine.Log.Info($"Running test class {declaringType}...", MessageSource.Test);
			object? currentClassInstance = Activator.CreateInstance(declaringType);

			if (declaringType.GetCustomAttribute<TestClassRunParallel>() != null)
			{
				Engine.Log.Info($"=-= Parallel Execution =-=", MessageSource.Test);

				var tasks = new Task[functions.Count];
				for (var i = 0; i < functions.Count; i++)
				{
					MethodInfo func = functions[i];
					tasks[i] = Task.Run(() =>
					{
						Engine.Log.Info($"  Running test {func.Name}...", MessageSource.Test);
						func.Invoke(currentClassInstance, new object[] { });
						completed++;
					});
				}

				Task.WaitAll(tasks, 10_000);
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
					}
					catch (Exception)
					{
						// ignored, it's printed by the internal engine error handling
					}
				}
			}

			// The format of this message must match the old system's regex!
			Engine.Log.Info($"Test completed: {completed}/{total}!", MessageSource.Test);
		}
	}

	private static async Task RunTestScenes(List<Type> testScenes)
	{
		for (var i = 0; i < testScenes.Count; i++)
		{
			Type sceneType = testScenes[i];
			object? sceneInstance = Activator.CreateInstance(sceneType);
			if (sceneInstance is not TestingScene sc)
			{
				Engine.Log.Error($"Couldn't initialize test scene of type {sceneType.Name}", MessageSource.Test);
				continue;
			}

			Engine.Log.Info($"Running test scene {sceneType}...", MessageSource.Test);
			await Engine.SceneManager.SetScene(sc);
			Func<IEnumerator>[] testRoutines = sc.GetTestCoroutines();
			foreach (Func<IEnumerator> testRoutine in testRoutines)
			{
				MethodInfo routineReflect = testRoutine.GetMethodInfo();
				string functionName = routineReflect.Name;
				Engine.Log.Info($"  Running test {functionName}...", MessageSource.Test);

				IEnumerator enumerator = testRoutine();
				Coroutine coroutine = Engine.CoroutineManager.StartCoroutine(enumerator);
				while (!coroutine.Finished && !coroutine.Stopped)
				{
					if (coroutine.CurrentWaiter is TestWaiterRunLoops runLoopsWaiter && !runLoopsWaiter.Finished)
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
			}

			Engine.Log.Info($"Passed {sceneType}!", MessageSource.Test);
		}
	}
}