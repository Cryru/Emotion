#nullable enable

namespace Emotion.Game.Time.Routines;

public class AsyncJobCoroutineManager : CoroutineManager
{
    private ManualResetEventSlim _lock = new ManualResetEventSlim();

    public AsyncJobCoroutineManager() : base(false)
    {
        SupportsTime = false;
    }

    public override void Update(float timePassed = 0)
    {
        base.Update(timePassed);

        if (_runningRoutines.Count == 0)
            _lock.Wait();
        _lock.Reset();
    }

    public override Coroutine StartCoroutine(IEnumerator enumerator)
    {
        Coroutine newRoutine = base.StartCoroutine(enumerator);
        _lock.Set();
        return newRoutine;
    }
}
