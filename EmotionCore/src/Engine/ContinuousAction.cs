// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;

#endregion

namespace Emotion.Engine
{
    /// <summary>
    /// An action which is executed after something else finishes.
    /// </summary>
    public struct ContinuousAction
    {
        private Action _action;

        /// <summary>
        /// Perform an action when its done.
        /// </summary>
        /// <param name="action">The action to execute afterward.</param>
        public void Then(Action action)
        {
            _action = action;
        }

        /// <summary>
        /// When the primary action is done and the action needs to be called.
        /// </summary>
        public void Done()
        {
            _action?.Invoke();
            _action = null;
        }
    }
}