#region Using

using System;
using System.Collections.Concurrent;
using System.Threading;

#endregion

namespace Emotion.Common.Threading
{
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
        private ConcurrentQueue<EmAction> _queue = new ConcurrentQueue<EmAction>();

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

            while (!_queue.IsEmpty)
            {
                bool dequeued = _queue.TryDequeue(out EmAction task);
                if (dequeued) task.Run();
                else _queue.Enqueue(task);
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
        public EmAction ExecuteOnThread(Action action)
        {
            // Wrap the action in a task.
            EmAction completionTask = new EmAction(action);

            // Check if on the managed thread.
            if (IsManagedThread())
            {
                completionTask.Run();
                return completionTask;
            }

            // Add the action to the queue.
            _queue.Enqueue(completionTask);
            return completionTask;
        }

        /// <summary>
        /// Check whether the executing thread is the managed thread. If it isn't, an exception is thrown.
        /// </summary>
        public void IsThreadOrError()
        {
            if (!IsManagedThread()) throw new Exception($"Not currently executing on the {ThreadName} thread.");
        }

        #endregion
    }
}