// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Emotion.System
{
    public sealed class ThreadManager
    {
        /// <summary>
        /// The id of the GL thread.
        /// </summary>
        private static int _glThreadId;

        /// <summary>
        /// The queue of actions to execute on the GL thread.
        /// </summary>
        private static Queue<Action> _queue = new Queue<Action>();

        /// <summary>
        /// Binds the current thread as the GL thread.
        /// </summary>
        internal static void BindThread()
        {
            _glThreadId = Thread.CurrentThread.ManagedThreadId;
            Thread.CurrentThread.Name = "GL Thread";
        }

        /// <summary>
        /// Performs queued tasks on the GL thread.
        /// </summary>
        internal static void Run()
        {
            // Check if on GL thread.
            if (!IsGLThread()) throw new Exception("The GL thread has changed.");

            lock (_queue)
            {
                while (_queue.Count > 0) _queue.Dequeue()();
            }
        }

        #region API

        /// <summary>
        /// Returns whether the executing thread is the GL thread.
        /// </summary>
        /// <returns>True if the thread on which this is called is the GL thread, false otherwise.</returns>
        public static bool IsGLThread()
        {
            return Thread.CurrentThread.ManagedThreadId == _glThreadId;
        }

        /// <summary>
        /// Execute the action on the GL thread. Will block the current thread until ready.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void ExecuteGLThread(Action action)
        {
            // Check if on the GL thread.
            if (IsGLThread())
            {
                action();
                return;
            }

            // Wrap and add the action.
            bool done = false;
            lock (_queue)
            {
                _queue.Enqueue(() =>
                {
                    action();
                    done = true;
                });
            }

            // Block until the action is executed.
            while (!done) Task.Delay(1).Wait();
        }

        /// <summary>
        /// Check whether the executing thread is the GL thread. If it's not an exception is thrown.
        /// </summary>
        public static void ForceGLThread()
        {
            if (!IsGLThread()) throw new Exception("Not currently executing on the GL thread.");
        }

        #endregion
    }
}