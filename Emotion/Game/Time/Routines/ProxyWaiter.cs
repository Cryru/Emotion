#region Using

using System;

#endregion

namespace Emotion.Game.Time.Routines
{
    /// <summary>
    /// A routine waiter that executes a callback.
    /// </summary>
    public class ProxyWaiter : IRoutineWaiter
    {
        public bool Finished { get; protected set; }

        private Func<bool> _callback;

        public ProxyWaiter(Func<bool> callback)
        {
            _callback = callback;
        }

        public void Update()
        {
            Finished = _callback();
        }
    }
}