// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;

#endregion

namespace Emotion.Engine.Threading
{
    public static class ALThread
    {
        private static ThreadManager _threadManager;

        static ALThread()
        {
            _threadManager = new ThreadManager("AL Thread");
        }

        /// <summary>
        /// Binds the current thread as the AL thread.
        /// </summary>
        internal static void BindThread()
        {
            _threadManager.BindThread();
        }

        /// <summary>
        /// Performs queued tasks on the AL thread.
        /// </summary>
        internal static void Run()
        {
            _threadManager.Run();
        }

        #region API

        /// <summary>
        /// Returns whether the executing thread is the AL thread.
        /// </summary>
        /// <returns>True if the thread on which this is called is the AL thread, false otherwise.</returns>
        public static bool IsALThread()
        {
            return _threadManager.IsManagedThread();
        }

        /// <summary>
        /// Execute the action on the AL thread. Will block the current thread until ready.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void ExecuteALThread(Action action)
        {
            _threadManager.ExecuteOnThread(action);
        }

        /// <summary>
        /// Check whether the executing thread is the AL thread. If it's not an exception is thrown.
        /// </summary>
        public static void ForceALThread()
        {
            _threadManager.ForceThread();
        }

        #endregion
    }
}