#region Using

using Emotion.Core.Utility.Coroutines;
using Emotion.Standard.DataStructures;


#endregion

namespace Emotion.Core.Utility.Threading
{
    internal class ManagedThreadInvocationBase : IRoutineWaiter
    {
        public bool Finished { get; private set; }

        public void Update()
        {

        }

        public virtual void Run()
        {
            Finished = true;
        }
    }

    internal class ManagedThreadInvocationResult<T> : ManagedThreadInvocationBase
    {
        public T Result;
        public Func<T> Action;

        public ManagedThreadInvocationResult(Func<T> action)
        {
            Action = action;
        }

        public override void Run()
        {
            Result = Action();
            base.Run();
        }
    }

    internal class ManagedThreadInvocationResult<T, T1> : ManagedThreadInvocationBase
    {
        public T Result;
        public T1 Arg1;
        public Func<T1, T> Action;

        public ManagedThreadInvocationResult(Func<T1, T> action, T1 arg1)
        {
            Arg1 = arg1;
            Action = action;
        }

        public override void Run()
        {
            Result = Action(Arg1);
            base.Run();
        }
    }

    internal class ManagedThreadInvocation : ManagedThreadInvocationBase
    {
        public Action Action;

        public ManagedThreadInvocation(Action action)
        {
            Action = action;
        }

        public override void Run()
        {
            Action();
            base.Run();
        }
    }

    internal class ManagedThreadInvocation<T1> : ManagedThreadInvocationBase
    {
        public T1 Arg1;
        public Action<T1> Action;

        public ManagedThreadInvocation(Action<T1> action, T1 arg1)
        {
            Arg1 = arg1;
            Action = action;
        }

        public override void Run()
        {
            Action(Arg1);
            base.Run();
        }
    }

    internal class ManagedThreadInvocation<T1, T2> : ManagedThreadInvocationBase
    {
        public T1 Arg1;
        public T2 Arg2;
        public Action<T1, T2> Action;

        public ManagedThreadInvocation(Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Action = action;
        }

        public override void Run()
        {
            Action(Arg1, Arg2);
            base.Run();
        }
    }

    internal class ManagedThreadInvocationResult<T, T1, T2> : ManagedThreadInvocationBase
    {
        public T Result;
        public T1 Arg1;
        public T2 Arg2;
        public Func<T1, T2, T> Action;

        public ManagedThreadInvocationResult(Func<T1, T2, T> action, T1 arg1, T2 arg2)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Action = action;
        }

        public override void Run()
        {
            Result = Action(Arg1, Arg2);
            base.Run();
        }
    }

    internal class ManagedThreadInvocationResult<T, T1, T2, T3> : ManagedThreadInvocationBase
    {
        public T Result;
        public T1 Arg1;
        public T2 Arg2;
        public T3 Arg3;
        public Func<T1, T2, T3, T> Action;

        public ManagedThreadInvocationResult(Func<T1, T2, T3, T> action, T1 arg1, T2 arg2, T3 arg3)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Action = action;
        }

        public override void Run()
        {
            Result = Action(Arg1, Arg2, Arg3);
            base.Run();
        }
    }
}