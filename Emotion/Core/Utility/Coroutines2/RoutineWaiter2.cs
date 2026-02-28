#nullable enable

using System.Threading.Tasks.Sources;

namespace Emotion.Core.Utility.Coroutines2;

public sealed class RoutineWaiter2 : IValueTaskSource<bool>
{
    #region IValueTaskSource

    public short Version => _core.Version;

    private ManualResetValueTaskSourceCore<bool> _core;

    public void Initialize(float time)
    {
        _core.Reset(); // increments version, removes old continuation
        CurrentWaiter_Time = time;
        Finished = false;
    }

    public void Complete()
    {
        _core.SetResult(true);
    }

    public bool GetResult(short token)
    {
        bool result = _core.GetResult(token);
        Finished = true; // Its over
        return result;
    }

    public ValueTaskSourceStatus GetStatus(short token)
    {
        return _core.GetStatus(token);
    }

    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
    {
        _core.OnCompleted(continuation, state, token, flags);
    }

    #endregion

    #region Coroutine Logic

    public bool Finished { get; private set; }

    public float CurrentWaiter_Time;

    public void CoroutineRun(float dt)
    {
        if (CurrentWaiter_Time != 0)
        {
            CurrentWaiter_Time -= dt;
            if (CurrentWaiter_Time <= 0) Complete();
        }
        else // Single tick
        {
            Complete();
        }
    }

    #endregion
}