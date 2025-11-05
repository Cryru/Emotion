#region Using

using System.Collections.Concurrent;

#endregion

namespace Emotion.Core.Utility.Threading
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

        private ConcurrentQueue<ManagedThreadInvocationBase> _invocationQueue = new ConcurrentQueue<ManagedThreadInvocationBase>();

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

            // Run old actions queue.
            while (!_queue.IsEmpty)
            {
                bool dequeued = _queue.TryDequeue(out EmAction task);
                if (dequeued) task.Run();
            }

            // Run new actions queue.
            while (!_invocationQueue.IsEmpty)
            {
                bool dequeued = _invocationQueue.TryDequeue(out ManagedThreadInvocationBase task);
                if (dequeued) task.Run();
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

        #region Old Execution API

        /// <summary>
        /// Execute the action on the managed thread. Will block the current thread until ready.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public EmAction ExecuteOnThread(Action action)
        {
            // Wrap the action in a task.
            var completionTask = new EmAction(action);

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

        #endregion

        #region New Execution API

        /// <summary>
        /// Execute a function on the managed thread, and get its result.
        /// </summary>
        public T ExecuteOnThread<T>(Func<T> action)
        {
            // Run straight away if on the managed thread.
            if (IsManagedThread()) return action();

            var invocation = new ManagedThreadInvocationResult<T>(action);
            _invocationQueue.Enqueue(invocation);
            invocation.Wait();
            return invocation.Result;
        }

        /// <summary>
        /// Execute a function on the managed thread without blocking.
        /// </summary>
        public ThreadExecutionWaitToken ExecuteOnThreadAsync(Action action)
        {
            // Run straight away if on the managed thread.
            if (IsManagedThread())
            {
                action();
                return ThreadExecutionWaitToken.FinishedByDefault;
            }

            var invocation = new ManagedThreadInvocation(action);
            _invocationQueue.Enqueue(invocation);
            return invocation;
        }

        /// <inheritdoc cref="ExecuteOnThreadAsync(Action)" />
        public ThreadExecutionWaitToken ExecuteOnThreadAsync<T>(Action<T> action, T arg1)
        {
            // Run straight away if on the managed thread.
            if (IsManagedThread())
            {
                action(arg1);
                return ThreadExecutionWaitToken.FinishedByDefault;
            }

            var invocation = new ManagedThreadInvocation<T>(action, arg1);
            _invocationQueue.Enqueue(invocation);
            return invocation;
        }

        /// <inheritdoc cref="ExecuteOnThreadAsync(Action)" />
        public void ExecuteOnThreadAsync<T, T2>(Action<T, T2> action, T arg1, T2 arg2)
        {
            // Run straight away if on the managed thread.
            if (IsManagedThread())
            {
                action(arg1, arg2);
                return;
            }

            var invocation = new ManagedThreadInvocation<T, T2>(action, arg1, arg2);
            _invocationQueue.Enqueue(invocation);
        }

        /// <inheritdoc cref="ExecuteOnThreadAsync(Action)" />
        public T ExecuteOnThread<T, T1>(Func<T1, T> action, T1 arg1)
        {
            // Run straight away if on the managed thread.
            if (IsManagedThread()) return action(arg1);

            var invocation = new ManagedThreadInvocationResult<T, T1>(action, arg1);
            _invocationQueue.Enqueue(invocation);
            invocation.Wait();
            return invocation.Result;
        }

        /// <inheritdoc cref="ExecuteOnThreadAsync(Action)" />
        public T ExecuteOnThread<T, T1, T2>(Func<T1, T2, T> action, T1 arg1, T2 arg2)
        {
            // Run straight away if on the managed thread.
            if (IsManagedThread()) return action(arg1, arg2);

            var invocation = new ManagedThreadInvocationResult<T, T1, T2>(action, arg1, arg2);
            _invocationQueue.Enqueue(invocation);
            invocation.Wait();
            return invocation.Result;
        }

        /// <inheritdoc cref="ExecuteOnThreadAsync(Action)" />
        public T ExecuteOnThread<T, T1, T2, T3>(Func<T1, T2, T3, T> action, T1 arg1, T2 arg2, T3 arg3)
        {
            // Run straight away if on the managed thread.
            if (IsManagedThread()) return action(arg1, arg2, arg3);

            var invocation = new ManagedThreadInvocationResult<T, T1, T2, T3>(action, arg1, arg2, arg3);
            _invocationQueue.Enqueue(invocation);
            invocation.Wait();
            return invocation.Result;
        }

        #endregion
    }
}