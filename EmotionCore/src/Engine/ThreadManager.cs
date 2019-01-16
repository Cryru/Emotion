// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Emotion.Engine
{
    /// <summary>
    /// Manages execution on a specific frame.
    /// </summary>
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
        private ConcurrentQueue<Task> _queue = new ConcurrentQueue<Task>();

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
        public void BindThread()
        {
            _threadId = Thread.CurrentThread.ManagedThreadId;
            if (Thread.CurrentThread.Name == null) Thread.CurrentThread.Name = ThreadName;
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
                bool dequeued = _queue.TryDequeue(out Task task);
                if (dequeued) task.RunSynchronously();
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
        public Task ExecuteOnThread(Action action)
        {
            // Wrap the action in a task.
            Task completionTask = new Task(action);

            // Check if on the managed thread.
            if (IsManagedThread())
            {
                completionTask.RunSynchronously();
                return completionTask;
            }

            // Add the action to the queue.
            _queue.Enqueue(completionTask);

            // Block until the task is executed.
            if (!BlockOnExecution) return completionTask;
            completionTask.Wait();
            return completionTask;
        }

        /// <summary>
        /// Check whether the executing thread is the managed thread. If it isn't, an exception is thrown.
        /// </summary>
        public void ForceThread()
        {
            if (!IsManagedThread()) throw new Exception($"Not currently executing on the {ThreadName} thread.");
        }

        #endregion
    }
}