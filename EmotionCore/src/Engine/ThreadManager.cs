// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Threading;

#endregion

namespace Emotion.Engine
{
    internal class ThreadManager
    {
        private static int _glThreadId;

        private static List<Action> _queue = new List<Action>();

        /// <summary>
        /// Binds the current thread as the GL thread.
        /// </summary>
        internal static void BindThread()
        {
            _glThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Returns whether the executing thread is the GL thread.
        /// </summary>
        /// <returns>True if the thread on which this is called is the GL thread, false otherwise.</returns>
        internal static bool IsGLThread()
        {
            return Thread.CurrentThread.ManagedThreadId == _glThreadId;
        }

        /// <summary>
        /// Execute the action on the GL thread. Will block the current thread until ready.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        internal static void ExecuteGLThread(Action action)
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
                _queue.Add(() =>
                {
                    action();
                    done = true;
                });
            }

            // Block until ready.
            while (!done) Thread.Sleep(1);
        }

        internal static void Run()
        {
            // Check if on GL thread.
            if (!IsGLThread()) throw new Exception("The GL thread has changed.");

            lock (_queue)
            {
                foreach (Action action in _queue)
                {
                    action();
                }

                _queue.Clear();
            }
        }
    }
}