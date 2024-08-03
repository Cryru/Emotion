#nullable enable

#region Using

using Emotion.Game.Time.Routines;

#endregion

namespace Emotion.Testing;

public class VerifyScreenshotResult : IRoutineWaiter
{
    public bool Passed;

    public VerifyScreenshotResult(bool passed)
    {
        Passed = passed;
    }

    public bool Finished { get; set; }

    public void Update()
    {
        Finished = true;
    }
}