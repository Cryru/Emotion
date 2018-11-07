// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Emotion.Engine.Threading
{
    public class ThreadManager
    {
        #region Properties

        /// <summary>
        /// The name of the thread being managed.
        /// </summary>
        public string ThreadName { get; private set; }
        
        /// <summary>
        /// Whether to block the current thread when ExecuteOnThread is called.
        /// </summary>
        public bool BlockOnExecution { get; set; } = true;

        #endregion

        /// <summary>
        /// The id of the managed thread.
        /// </summary>
        private int _threadId;

        /// <summary>
        /// The queue of actions to execute on the thread.
        /// </summary>
        private Queue<Action> _queue = new Queue<Action>();

        /// <summary>
        /// Initiate a thread manager.
        /// </summary>
        /// <param name="name">The name of the thread which will be managed.</param>
        public ThreadManager(string name)
        {
            ThreadName = name;
        }

        /// <summary>
        /// Binds the current thread as the managed thread.
        /// </summary>
        internal void BindThread()
        {
            _threadId = Thread.CurrentThread.ManagedThreadId;
            Thread.CurrentThread.Name = ThreadName;
        }

        /// <summary>
        /// Performs queued tasks on the managed thread.
        /// </summary>
        internal void Run()
        {
            // Check if on the managed thread.
            if (!IsManagedThread()) throw new Exception("The managed thread has changed.");

            lock (_queue)
            {
                while (_queue.Count > 0) _queue.Dequeue()();
            }
        }

        #region API

        /// <summary>
        /// Returns whether the executing thread is the managed thread.
        /// </summary>
        /// <returns>True if the thread on which this is called is the managed thread, false otherwise.</returns>
        public bool IsManagedThread()
        {
            return Thread.CurrentThread.ManagedThreadId == _threadId;
        }

        /// <summary>
        /// Execute the action on the managed thread. Will block the current thread until ready.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public void ExecuteOnThread(Action action)
        {
            // Check if on the managed thread.
            if (IsManagedThread())
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
            if (!BlockOnExecution) return;
            while (!done) Task.Delay(1).Wait();
        }

        /// <summary>
        /// Check whether the executing thread is the managed thread. If it isn't, an exception is thrown.
        /// </summary>
        public void ForceThread()
        {
            if (!IsManagedThread()) throw new Exception("Not currently executing on the managed thread.");
        }

        #endregion
    }
}