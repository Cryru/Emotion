using Emotion.Core.Utility.Coroutines;

namespace Emotion.Core.Utility.Threading;

#nullable enable

public class ThreadExecutionWaitToken : IRoutineWaiter
{
    public static ThreadExecutionWaitToken FinishedByDefault = new ThreadExecutionWaitToken().Trigger();

    public bool Finished { get; private set; }

    public void Update()
    {
        
    }

    public ThreadExecutionWaitToken Trigger()
    {
        Finished = true;
        return this;
    }
}
