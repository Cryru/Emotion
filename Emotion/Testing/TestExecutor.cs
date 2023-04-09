#region Using

using System.Collections;
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
	/// Signifies that the tests are being ran in a CI environment.
	/// Changes error handling and prevents hang ups used by debugging etc.
	/// </summary>
	public static bool RunningTestsInCI = false;

	public static void ExecuteTests(string[] args)
	{
		// todo: read args and start running split processes, different configs etc.

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

		Task.Run(async () => { await TestingThread(testScenes); });
	}

	private static async Task TestingThread(List<Type> testScenes)
	{
		for (var i = 0; i < testScenes.Count; i++)
		{
			Type sceneType = testScenes[i];
			object? sceneInstance = Activator.CreateInstance(sceneType);
			if (sceneInstance is not TestingScene sc)
			{
				Engine.Log.Error($"Couldn't initialize test scene of type {sceneType.Name}", "Test");
				continue;
			}

			await Engine.SceneManager.SetScene(sc);
			Func<IEnumerator>[] testRoutines = sc.GetTestCoroutines();
			foreach (Func<IEnumerator> testRoutine in testRoutines)
			{
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

				// todo: have the test routines be classes rather than just functions so they can contain name and meta
				Engine.Log.Info("Test routine completed", "Test");
			}
		}
	}

	public static void SetupConfigForTests(Configurator config)
	{
		config.DebugMode = true;

		// todo: test logger
	}
}