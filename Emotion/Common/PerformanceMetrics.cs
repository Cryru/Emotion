#region Using

using System.Diagnostics;

#endregion

#nullable enable

namespace Emotion.Common;

public static class PerformanceMetrics
{
	public static float[] TickRate = new float[100];
	public static float[] FrameDelta = new float[100];

	private static int _tickRateRingIdx;
	private static int _frameRateRingIdx;

	/// <summary>
	/// The frames per second pushed in the last second.
	/// </summary>
	public static int FpsLastSecond { get; private set; }

	/// <summary>
	/// How many frames passed since the last update.
	/// </summary>
	public static int FramesPerUpdateLastSecond { get; private set; }

	private static Stopwatch _updateTimer = new Stopwatch();
	private static Stopwatch _frameTimer = new Stopwatch();
	private static int _fpsUpdateSecond;
	private static int _frameCounterThisSecond;
	private static int _framesPerUpdate;

	[Conditional("DEBUG")]
	public static void RegisterTick()
	{
		TickRate[_tickRateRingIdx] = _updateTimer.ElapsedMilliseconds;
		_updateTimer.Restart();
		_tickRateRingIdx++;
		if (_tickRateRingIdx > TickRate.Length - 1) _tickRateRingIdx = 0;

		_framesPerUpdate = 0;
	}

	[Conditional("DEBUG")]
	public static void RegisterFrame()
	{
		FrameDelta[_frameRateRingIdx] = _frameTimer.ElapsedMilliseconds;
		_frameTimer.Restart();
		_frameRateRingIdx++;
		if (_frameRateRingIdx > FrameDelta.Length - 1) _frameRateRingIdx = 0;

		_framesPerUpdate++;

		int currentSecond = DateTime.Now.Second;
		if (currentSecond != _fpsUpdateSecond)
		{
			FpsLastSecond = _frameCounterThisSecond;
			FramesPerUpdateLastSecond = _framesPerUpdate;
			_frameCounterThisSecond = 0;
			_fpsUpdateSecond = currentSecond;
		}

		_frameCounterThisSecond++;
	}
}