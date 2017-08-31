// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Soul.Engine.Internal
{
    internal class AsyncScript
    {
        #region Internals

        /// <summary>
        /// The thread the script is running on.
        /// </summary>
        private Task _thread;

        /// <summary>
        /// The timeout tracking timer.
        /// </summary>
        private int _timeoutTimer;

        /// <summary>
        /// The token used to cancel the thread.
        /// </summary>
        private CancellationTokenSource _token;

        #endregion

        #region Information

        /// <summary>
        /// Whether the thread is running.
        /// </summary>
        public bool isRunning
        {
            get { return _thread.Status == TaskStatus.Running; }
        }

        #endregion

        /// <summary>
        /// Creates a new asynchronous script.
        /// </summary>
        /// <param name="thread">The thread the script is running on.</param>
        /// <param name="token">The token for canceling the thread.</param>
        /// <param name="timeout">The timeout for the script.</param>
        public AsyncScript(Task thread, CancellationTokenSource token, int timeout)
        {
            _thread = thread;
            _token = token;
            _timeoutTimer = timeout;
        }

        /// <summary>
        /// Updates the time for the script.
        /// </summary>
        /// <returns>Whether the thread has timed out.</returns>
        public bool UpdateTime()
        {
            _timeoutTimer -= Globals.Context.FrameTime;

            if (_timeoutTimer <= 0)
            {
                Error.Raise(51, "A script thread has timed out.");
                Stop();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stops the running script.
        /// </summary>
        public void Stop()
        {
            _token.Cancel();
        }
    }
}