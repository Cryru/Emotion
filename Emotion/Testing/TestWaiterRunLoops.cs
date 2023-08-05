#region Using

using Emotion.Game.Time.Routines;

#endregion

#nullable enable

namespace Emotion.Testing;

/// <summary>
/// Used by test coroutines to specify they want a select number of
/// update and draw loops of the testing scene to be ran.
/// If the number of loops to run is set to -1 then the test will run the scene normally,
/// and it will never finish.
/// </summary>
public class TestWaiterRunLoops : IRoutineWaiter
{
	/// <summary>
	/// Whether the number of loops requested have ran.
	/// </summary>
	public bool Finished
	{
		get => _loopsCount >= LoopsToRun && LoopsToRun != -1;
	}

	/// <summary>
	/// How many loops to run.
	/// </summary>
	public int LoopsToRun { get; init; }

	private int _loopsCount;

	public TestWaiterRunLoops(int loopsToRun)
	{
		if (loopsToRun == -1 && !TestExecutor.AllowInfiniteLoops)
		{
			Engine.Log.Error("Tried to run infinite loops in CI.", "Test");
			loopsToRun = 1;
		}

		LoopsToRun = loopsToRun;
	}

	public void Update()
	{
	}

	public void AddLoopRan()
	{
		_loopsCount++;
	}
}