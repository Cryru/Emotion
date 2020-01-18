#region Using

using System;
using System.Runtime.CompilerServices;
using System.Threading;

#endregion

namespace Emotion.Common.Threading
{
    /// <summary>
    /// A token object used to await and chain actions.
    /// This is used rather than the default C# Task because it gives you control over which thread it will execute on.
    /// </summary>
    public class EmAction : INotifyCompletion
    {
        private Action _contAction;
        private Action _actionToExec;
        private ManualResetEvent _mutex;
        private bool _ran;

        /// <summary>
        /// An awaitable action which will execute on the same thread on which it is ran.
        /// This is an already completed one.
        /// </summary>
        public EmAction(bool ran)
        {
            _mutex = new ManualResetEvent(ran);
            _ran = ran;
        }

        /// <summary>
        /// An awaitable action which will execute on the same thread on which it is ran.
        /// This is an empty task which will not execute anything but can be used as a mutex.
        /// </summary>
        public EmAction() : this(false)
        {
        }

        /// <summary>
        /// An awaitable action which will execute on the same thread on which it is ran.
        /// </summary>
        /// <param name="action">The action to execute when Run() is invoked.</param>
        public EmAction(Action action) : this(false)
        {
            _contAction = null;
            _actionToExec = action;
        }

        /// <summary>
        /// Perform an action when its done.
        /// </summary>
        /// <param name="action">The action to execute afterward.</param>
        public void ContinueWith(Action action)
        {
            if (_mutex == null) return;

            // Check if a continuation is already set.
            if (_contAction != null)
            {
                Action oldAction = _contAction;
                _contAction = () =>
                {
                    oldAction();
                    action();
                };
                return;
            }

            _contAction = action;

            // Check if ran.
            if (_ran) _contAction?.Invoke();
        }

        /// <summary>
        /// Run the task.
        /// </summary>
        public void Run()
        {
            // Invoke the task action.
            _actionToExec?.Invoke();
            _actionToExec = null;
            // Release the holder.
            _mutex.Set();
            _mutex = null;
            // Run next action.
            _contAction?.Invoke();
            _contAction = null;
        }

        /// <summary>
        /// Blocks the thread until the task is run.
        /// </summary>
        public void Wait()
        {
            _mutex?.WaitOne();
        }

        #region Async Await

        public bool IsCompleted
        {
            get => _ran || _mutex == null;
        }

        public void OnCompleted(Action continuation)
        {
            continuation?.Invoke();
        }

        public EmAction GetAwaiter()
        {
            return this;
        }

        public void GetResult()
        {
        }

        #endregion
    }
}