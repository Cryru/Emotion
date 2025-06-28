#region Using

using System.Threading;

#endregion

#nullable enable

namespace Emotion.Common.Threading
{
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
        public static void BindThread()
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

        /// <summary>
        /// Execute the action on the GL thread. Will block the current thread until ready.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void ExecuteGLThread(Action action)
        {
            _threadManager.ExecuteOnThread(action).Wait();
        }

        /// <summary>
        /// Execute the action on the GL thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static EmAction ExecuteGLThreadAsync(Action action)
        {
            return _threadManager.ExecuteOnThread(action);
        }

        /// <summary>
        /// Check whether the executing thread is the GL thread. If it's not an exception is thrown.
        /// </summary>
        public static void ForceGLThread()
        {
            _threadManager.IsThreadOrError();
        }

        #endregion

        #region New API

        /// <summary>
        /// Execute the action on the GL thread and get its result. Will block the current thread until ready.
        /// </summary>
        public static T ExecuteGLThread<T>(Func<T> action)
        {
            return _threadManager.ExecuteOnThread(action);
        }

        public static ThreadExecutionWaitToken ExecuteOnGLThreadAsync(Action action)
        {
            return _threadManager.ExecuteOnThreadAsync(action);
        }

        /// <summary>
        /// Execute the action on the GL thread without blocking.
        /// </summary>
        public static ThreadExecutionWaitToken ExecuteOnGLThreadAsync<T1>(Action<T1> action, T1 arg1)
        {
            return _threadManager.ExecuteOnThreadAsync(action, arg1);
        }

        /// <inheritdoc cref="ExecuteOnGLThreadAsync{T}(Action{T}, T)" />
        public static void ExecuteOnGLThreadAsync<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            _threadManager.ExecuteOnThreadAsync(action, arg1, arg2);
        }

        /// <inheritdoc cref="ExecuteGLThread{T}(Func{T})" />
        public static T ExecuteGLThread<T, T1>(Func<T1, T> action, T1 arg1)
        {
            return _threadManager.ExecuteOnThread(action, arg1);
        }

        /// <inheritdoc cref="ExecuteGLThread{T}(Func{T})" />
        public static T ExecuteGLThread<T, T1, T2>(Func<T1, T2, T> action, T1 arg1, T2 arg2)
        {
            return _threadManager.ExecuteOnThread(action, arg1, arg2);
        }

        /// <inheritdoc cref="ExecuteGLThread{T}(Func{T})" />
        public static T ExecuteGLThread<T, T1, T2, T3>(Func<T1, T2, T3, T> action, T1 arg1, T2 arg2, T3 arg3)
        {
            return _threadManager.ExecuteOnThread(action, arg1, arg2, arg3);
        }

        #endregion
    }
}