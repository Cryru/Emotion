// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Threading;

#endregion

namespace Emotion.Engine
{
    /// <summary>
    /// A token object used to chain actions.
    /// </summary>
    public class EmTask
    {
        private Action _contAction;
        private Action _actionToExec;
        private ManualResetEvent _mutex;

        /// <summary>
        /// An Emotion task which will execute on the same thread on which it is ran.
        /// This is an already completed task.
        /// </summary>
        public EmTask(bool ran)
        {
            _mutex = new ManualResetEvent(ran);
        }

        /// <summary>
        /// An Emotion task which will execute on the same thread on which it is ran.
        /// This is an empty task which will not execute anything but can be used as a mutex.
        /// </summary>
        public EmTask() : this(false)
        {
        }

        /// <summary>
        /// An Emotion task which will execute on the same thread on which it is ran.
        /// </summary>
        /// <param name="action">The action to execute when Run() is invoked.</param>
        public EmTask(Action action) : this(false)
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
    }
}