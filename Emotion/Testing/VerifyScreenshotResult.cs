#nullable enable

#region Using

using Emotion.Game.Routines;


#endregion

namespace Emotion.Testing;

public class VerifyScreenshotResult : IRoutineWaiter
{
    public bool Passed;

    public VerifyScreenshotResult(bool passed)
    {
        Passed = passed;
        if (!passed) TestExecutor.SetCurrentTestSceneTestAsFailed();
    }

    public bool Finished { get; set; }

    public void Update()
    {
        Finished = true;
    }
}