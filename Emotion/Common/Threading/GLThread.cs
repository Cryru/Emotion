#region Using

using System;
using System.Threading;

#endregion

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

        private static ManagedThread _threadManager;

        static GLThread()
        {
            _threadManager = new ManagedThread("GL Thread");
        }

        /// <summary>
        /// Binds the current thread as the GL thread.
        /// </summary>
        public static void BindThread()
        {
            if (Engine.Host?.NamedThreads ?? false) Thread.CurrentThread.Name ??= "GL Thread";
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
    }
}