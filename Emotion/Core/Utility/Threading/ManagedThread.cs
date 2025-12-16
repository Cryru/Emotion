#nullable enable

#region Using

using Emotion.Core.Utility.Coroutines;
using System.Collections.Concurrent;

#endregion

namespace Emotion.Core.Utility.Threading;

/// <summary>
/// Manages execution on a specific frame.
/// </summary>
public class ManagedThread
{
    #region Properties

    /// <summary>
    /// Whether the manager has been bound to a thread.
    /// </summary>
    public bool IsBound { get; private set; }

    /// <summary>
    /// The name of the thread being managed.
    /// </summary>
    public string ThreadName { get; private set; }

    /// <summary>
    /// Whether the thread manager has any actions pending.
    /// </summary>
    public bool Empty
    {
        get => _queue.IsEmpty;
    }

    #endregion

    /// <summary>
    /// The id of the managed thread.
    /// </summary>
    private int _threadId;

    /// <summary>
    /// The queue of actions to execute on the thread.
    /// </summary>
    private ConcurrentQueue<ManagedThreadInvocationBase> _queue = new();

    /// <summary>
    /// Initiate a thread manager.
    /// </summary>
    /// <param name="name">The name of the thread which will be managed.</param>
    public ManagedThread(string name)
    {
        ThreadName = name;
    }

    /// <summary>
    /// Binds the current thread as the managed thread.
    /// </summary>
    public void BindThread()
    {
        _threadId = Thread.CurrentThread.ManagedThreadId;
        if (Engine.Host?.NamedThreads ?? false) Thread.CurrentThread.Name ??= ThreadName;
        IsBound = true;
    }

    /// <summary>
    /// Performs queued tasks on the managed thread.
    /// </summary>
    public void Run()
    {
        // Check if on the managed thread.
        if (!IsManagedThread()) throw new Exception($"The {ThreadName} thread has changed.");

        // Run queue
        while (_queue.TryDequeue(out ManagedThreadInvocationBase? task))
        {
            task.Run();
        }
    }

    #region API

    /// <summary>
    /// Returns whether the executing thread is the managed thread.
    /// </summary>
    /// <returns>True if the thread on which this is called is the managed thread, false otherwise.</returns>
    public bool IsManagedThread()
    {
        return Environment.CurrentManagedThreadId == _threadId;
    }

    /// <summary>
    /// Check whether the executing thread is the managed thread. If it isn't, an exception is thrown.
    /// </summary>
    public void IsThreadOrError()
    {
        if (!IsManagedThread()) throw new Exception($"Not currently executing on the {ThreadName} thread.");
    }

    #endregion

    #region New Execution API

    /// <summary>
    /// Execute a function on the managed thread without blocking.
    /// </summary>
    public IRoutineWaiter ExecuteOnThreadAsync(Action action)
    {
        // Run straight away if on the managed thread.
        if (IsManagedThread())
        {
            action();
            return Coroutine.CompletedRoutine;
        }

        var invocation = new ManagedThreadInvocation(action);
        _queue.Enqueue(invocation);
        return invocation;
    }

    /// <inheritdoc cref="ExecuteOnThreadAsync(Action)" />
    public IRoutineWaiter ExecuteOnThreadAsync<T>(Action<T> action, T arg1)
    {
        // Run straight away if on the managed thread.
        if (IsManagedThread())
        {
            action(arg1);
            return Coroutine.CompletedRoutine;
        }

        var invocation = new ManagedThreadInvocation<T>(action, arg1);
        _queue.Enqueue(invocation);
        return invocation;
    }

    /// <inheritdoc cref="ExecuteOnThreadAsync(Action)" />
    public IRoutineWaiter ExecuteOnThreadAsync<T, T2>(Action<T, T2> action, T arg1, T2 arg2)
    {
        // Run straight away if on the managed thread.
        if (IsManagedThread())
        {
            action(arg1, arg2);
            return Coroutine.CompletedRoutine;
        }

        var invocation = new ManagedThreadInvocation<T, T2>(action, arg1, arg2);
        _queue.Enqueue(invocation);
        return invocation;
    }

    #endregion
}