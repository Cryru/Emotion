#nullable enable

namespace Emotion.Common;

public static class PerformanceMetrics
{
	public static float[] TickRate = new float[100];
	public static float[] FrameDelta = new float[100];

	public static int TickRateRingIdx;
	public static int FrameRateRingIdx;

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
		TickRate[TickRateRingIdx] = _updateTimer.ElapsedMilliseconds;
		_updateTimer.Restart();
		TickRateRingIdx++;
		if (TickRateRingIdx > TickRate.Length - 1) TickRateRingIdx = 0;

		_framesPerUpdate = 0;
	}

	[Conditional("DEBUG")]
	public static void RegisterFrame()
	{
		FrameDelta[FrameRateRingIdx] = _frameTimer.ElapsedMilliseconds;
		_frameTimer.Restart();
		FrameRateRingIdx++;
		if (FrameRateRingIdx > FrameDelta.Length - 1) FrameRateRingIdx = 0;

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