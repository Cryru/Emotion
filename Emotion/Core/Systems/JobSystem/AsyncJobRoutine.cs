#nullable enable

using Emotion.Core.Utility.Coroutines;

namespace Emotion.Core.Systems.JobSystem;

public class AsyncJobRoutine : IRoutineWaiter
{
    public bool Finished { get; set; }

    public string? JobTag { get; set; }

    public IEnumerator Routine;
    public IRoutineWaiter? CurrentWaiter;

    private bool _subRoutine;

    public AsyncJobRoutine(IEnumerator routine, bool subRoutine = false, string? tag = null)
    {
        Routine = routine;
        _subRoutine = subRoutine;
        JobTag = tag;
    }

    public void Update()
    {
        if (_subRoutine)
            RunTask();
    }

    public void RunTask()
    {
        Assert(!Finished);

        if (CurrentWaiter != null)
        {
            CurrentWaiter.Update();
            if (!CurrentWaiter.Finished) return;
            CurrentWaiter = null;
        }

        bool done = true;
        while (Routine.MoveNext())
        {
            object current = Routine.Current;
            if (current == null)
            {
                done = false;
                break;
            }

            if (current is IEnumerator subRoutine)
            {
                CurrentWaiter = new AsyncJobRoutine(subRoutine, true);
                done = false;
                break;
            }
            else if (current is IRoutineWaiter waiter)
            {
                waiter.Update();
                if (!waiter.Finished)
                {
                    CurrentWaiter = waiter;
                    done = false;
                    break;
                }
            }
        }

        if (done)
            Finished = true;
    }
}