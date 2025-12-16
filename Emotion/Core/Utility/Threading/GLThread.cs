#nullable enable

using Emotion.Core.Utility.Coroutines;

namespace Emotion.Core.Utility.Threading;

/// <summary>
/// A global thread manager bound to the thread on which the OpenGL context was created.
/// </summary>
public static class GLThread
{
    /// <summary>
    /// Whether the thread is bound.
    /// </summary>
    public static bool IsBound
    {
        get => _threadManager.IsBound;
    }

    /// <summary>
    /// Returns whether the queue is empty.
    /// </summary>
    public static bool Empty
    {
        get => _threadManager.Empty;
    }

    private static ManagedThread _threadManager;

    static GLThread()
    {
        _threadManager = new ManagedThread("GL");
    }

    /// <summary>
    /// Binds the current thread as the GL thread.
    /// </summary>
    internal static void BindThread()
    {
        if (Engine.Host?.NamedThreads ?? false) Thread.CurrentThread.Name ??= _threadManager.ThreadName;
        _threadManager.BindThread();
    }

    /// <summary>
    /// Performs queued tasks on the GL thread.
    /// </summary>
    public static void Run()
    {
        _threadManager.Run();
    }

    #region API

    /// <summary>
    /// Returns whether the executing thread is the GL thread.
    /// </summary>
    /// <returns>True if the thread on which this is called is the GL thread, false otherwise.</returns>
    public static bool IsGLThread()
    {
        return _threadManager.IsManagedThread();
    }

    #endregion

    #region New API

    /// <summary>
    /// Execute a function on GL thread - returns an awaitable token.
    /// </summary>
    public static IRoutineWaiter ExecuteOnGLThreadAsync(Action action)
    {
        return _threadManager.ExecuteOnThreadAsync(action);
    }

    /// <inheritdoc cref="ExecuteOnGLThreadAsync(Action)" />
    public static IRoutineWaiter ExecuteOnGLThreadAsync<T1>(Action<T1> action, T1 arg1)
    {
        return _threadManager.ExecuteOnThreadAsync(action, arg1);
    }

    /// <inheritdoc cref="ExecuteOnGLThreadAsync{T}(Action{T}, T)" />
    public static IRoutineWaiter ExecuteOnGLThreadAsync<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
    {
        return _threadManager.ExecuteOnThreadAsync(action, arg1, arg2);
    }

    #endregion
}