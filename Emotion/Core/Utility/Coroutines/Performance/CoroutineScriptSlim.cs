#nullable enable

namespace Emotion.Core.Utility.Coroutines.Performance;

public struct CoroutineScriptSlim : ICoroutineScript
{
    private SlimValueCoroutineWaiter[] _parts;
    internal int ExecutionAt = -1;

    public CoroutineScriptSlim(params SlimValueCoroutineWaiter[] parts)
    {
        _parts = parts;
    }

    public unsafe CoroutineScriptRunResult RunStep(Coroutine hostingRoutine)
    {
        ExecutionAt++;
        if (ExecutionAt >= _parts.Length) return CoroutineScriptRunResult.Finished;

        bool run = true;
        while (run)
        {
            run = false;

            ref SlimValueCoroutineWaiter waiter = ref _parts[ExecutionAt];
            switch (waiter.Type)
            {
                case SlimValueCoroutineWaiterType.Execute:
                {
                    var func = (delegate*<void>)waiter.FuncPtr;
                    func();
                    break;
                }
                case SlimValueCoroutineWaiterType.TimeWait when hostingRoutine.Parent.SupportsTime:
                {
                    var func = (delegate*<int>)waiter.FuncPtr;
                    int timeWait = func();
                    hostingRoutine.CurrentWaiter_Time = timeWait;
                    break;
                }
                case SlimValueCoroutineWaiterType.LoopOneBack:
                {
                    if (waiter.FuncPtr == (void*)IntPtr.Zero)
                    {
                        ExecutionAt--;
                        break;
                    }

                    var func = (delegate*<bool>)waiter.FuncPtr;
                    bool loop = func();
                    if (loop)
                    {
                        ExecutionAt--;
                    }

                    run = true; // Run again
                    break;
                }
            }
        }

        return CoroutineScriptRunResult.ContinueRunning;
    }
}