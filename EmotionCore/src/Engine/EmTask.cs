// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Threading.Tasks;

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
        private bool _isRan;

        /// <summary>
        /// An Emotion task which will execute on the same thread on which it is ran.
        /// This is an empty task which will not execute anything but can be used as a mutex.
        /// </summary>
        public EmTask()
        {
        }


        /// <summary>
        /// An Emotion task which will execute on the same thread on which it is ran.
        /// This is an already completed task.
        /// </summary>
        public EmTask(bool ran)
        {
            if (ran) _isRan = true;
        }

        /// <summary>
        /// An Emotion task which will execute on the same thread on which it is ran.
        /// </summary>
        /// <param name="action">The action to execute when Run() is invoked.</param>
        public EmTask(Action action)
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
            if (_isRan) return;

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
            _actionToExec?.Invoke();
            _actionToExec = null;
            _isRan = true;
            _contAction?.Invoke();
            _contAction = null;
        }

        /// <summary>
        /// Blocks the thread until the task is run.
        /// </summary>
        public void Wait()
        {
            while (!_isRan) Task.Delay(1).Wait();
        }
    }
}