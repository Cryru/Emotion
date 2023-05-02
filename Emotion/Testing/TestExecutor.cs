#nullable enable

#region Using

using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Emotion.Game.Time.Routines;
using Emotion.Utility;

#endregion

namespace Emotion.Testing;

public static class TestExecutor
{
	/// <summary>
	/// Whether we allow tests to halt with an infinite game loop waiter.
	/// Disable in CI and such.
	/// </summary>
	public static bool AllowInfiniteLoops = true;

	public static void ExecuteTests(string[] args, Configurator config = null)
	{
		// todo: read args and start running split processes, different configs etc.

		config ??= new Configurator();
		config.DebugMode = true;
		config.NoErrorPopup = true;
		config.Logger = new TestLogger("Logs");

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
			RunTestClasses(testFunctions);
			await RunTestScenes(testScenes);
			Engine.Quit();
		});

		Engine.Run();
	}

	private static void RunTestClasses(List<MethodInfo> testFunctions)
	{
		Type? currentClass = null;
		object? currentClassInstance = null;

		foreach (MethodInfo func in testFunctions)
		{
			// Create an instance of the test class.
			if (currentClass != func.DeclaringType)
			{
				if (currentClass != null) Engine.Log.Info($"Passed test class {currentClass}!", MessageSource.Test);
				currentClass = func.DeclaringType!;
				currentClassInstance = Activator.CreateInstance(currentClass);
				Engine.Log.Info($"Running test class {currentClass}...", MessageSource.Test);
			}

			// Run test.
			Engine.Log.Info($"  Running test {func.Name}...", MessageSource.Test);
			func.Invoke(currentClassInstance, new object[] { });
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