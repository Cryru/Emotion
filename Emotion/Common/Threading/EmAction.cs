#region Using

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Emotion.Game.Time.Routines;

#endregion

namespace Emotion.Common.Threading
{
    /// <summary>
    /// A token object used to await and chain actions.
    /// This is used rather than the default C# Task because it gives you control over which thread it will execute on.
    /// </summary>
    public class EmAction : INotifyCompletion, IRoutineWaiter
    {
        private Action _contAction;
        private Action _actionToExec;
        private ManualResetEvent _mutex;

        /// <summary>
        /// An awaitable action which will execute on the same thread on which it is ran.
        /// This is an already completed one.
        /// </summary>
        public EmAction(bool ran)
        {
            Finished = ran;
        }

        /// <summary>
        /// An awaitable action which will execute on the same thread on which it is ran.
        /// This is an empty task which will not execute anything but can be used as a mutex.
        /// </summary>
        public EmAction()
        {
            _mutex = new ManualResetEvent(false);
        }

        /// <summary>
        /// An awaitable action which will execute on the same thread on which it is ran.
        /// </summary>
        /// <param name="action">The action to execute when Run() is invoked.</param>
        public EmAction(Action action)
        {
            _contAction = null;
            _actionToExec = action;
            _mutex = new ManualResetEvent(false);
        }

        /// <summary>
        /// Perform an action when its done.
        /// </summary>
        /// <param name="action">The action to execute afterward.</param>
        public void ContinueWith(Action action)
        {
            // Check if already ran.
            if (Finished)
            {
                action?.Invoke();
                return;
            }

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
        }

        /// <summary>
        /// Run the task.
        /// </summary>
        public void Run()
        {
            // Invoke the task action.
            _actionToExec?.Invoke();
            _actionToExec = null;

            // Set finished.
            Finished = true;

            // Release the holder.
            _mutex?.Set();
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
            get => Finished;
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

        #region Coroutine Waiter

        public bool Finished { get; protected set; }

        public void Update()
        {
        }

        #endregion
    }
}