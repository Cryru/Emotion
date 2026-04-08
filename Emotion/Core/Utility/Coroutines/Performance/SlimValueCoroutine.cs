namespace Emotion.Core.Utility.Coroutines.Performance;

public enum SlimValueCoroutineWaiterType
{
    None,
    Execute,
    TimeWait,
    LoopOneBack
}

public unsafe struct SlimValueCoroutineWaiter
{
    public void* FuncPtr;
    public SlimValueCoroutineWaiterType Type;

    public static SlimValueCoroutineWaiter Execute(delegate*<void> func)
    {
        return new SlimValueCoroutineWaiter()
        {
            FuncPtr = func,
            Type = SlimValueCoroutineWaiterType.Execute
        };
    }

    public static SlimValueCoroutineWaiter TimeWait(delegate*<int> func)
    {
        return new SlimValueCoroutineWaiter()
        {
            FuncPtr = func,
            Type = SlimValueCoroutineWaiterType.TimeWait
        };
    }

    public static SlimValueCoroutineWaiter LoopOneBack(delegate*<bool> func)
    {
        return new SlimValueCoroutineWaiter()
        {
            FuncPtr = func,
            Type = SlimValueCoroutineWaiterType.LoopOneBack
        };
    }

    public static SlimValueCoroutineWaiter LoopOneBackInfinite()
    {
        return new SlimValueCoroutineWaiter()
        {
            FuncPtr = (void*)IntPtr.Zero,
            Type = SlimValueCoroutineWaiterType.LoopOneBack
        };
    }
}

internal class ___PROTO
{
    [System.Runtime.CompilerServices.InlineArray(SlimValueCoroutineActionArray.MaxWaiters)]
    public struct SlimValueCoroutineActionArray
    {
        public const int MaxWaiters = 8;

        private SlimValueCoroutineWaiter _element;
    }

    public enum SlimValueCoroutineState
    {
        None,
        WaitingForTime,
        Done
    }

    public struct SlimValueCoroutine
    {
        public SlimValueCoroutineState State;
        public int ExecutionAt;
        public int TimeWait;
        public SlimValueCoroutineActionArray Actions;

        public SlimValueCoroutine(params ReadOnlySpan<SlimValueCoroutineWaiter> parts)
        {
            State = SlimValueCoroutineState.None;
            for (int i = 0; i < parts.Length; i++)
            {
                Actions[i] = parts[i];
            }
        }
    }
}