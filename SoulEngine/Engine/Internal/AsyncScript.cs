// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System.Threading;
using Soul.Engine.Modules;

#endregion

namespace Soul.Engine.Internal
{
    internal class AsyncScript
    {
        #region Internals

        /// <summary>
        /// The thread the script is running on.
        /// </summary>
        private Thread _thread;

        /// <summary>
        /// The timeout tracking timer.
        /// </summary>
        private int _timeoutTimer;

        /// <summary>
        /// Whether the script is a looping one and should not timeout.
        /// </summary>
        private bool _looping;

        #endregion

        #region Information

        /// <summary>
        /// Whether the thread is running.
        /// </summary>
        public bool isRunning
        {
            get { return _thread.ThreadState == ThreadState.Running; }
        }

        #endregion

        /// <summary>
        /// Creates a new asynchronous script.
        /// </summary>
        /// <param name="script">The script code to run.</param>
        /// <param name="timeout">The timeout for the script.</param>
        /// <param name="looping">Whether the script is looping and should not timeout.</param>
        public AsyncScript(string script, int timeout, bool looping = false)
        {
            _thread = new Thread(() => ScriptEngine.RunScript(script));
            _timeoutTimer = timeout;
            _looping = looping;

            // Start the thread.
            _thread.Start();

            // Wait for the thread to start.
            while(!_thread.IsAlive) { }
        }

        /// <summary>
        /// Updates the timeout timer for the script.
        /// </summary>
        /// <returns>Whether the thread has timed out.</returns>
        public bool UpdateTime()
        {
            // Check if the thread exists.
            if (_thread == null) return true;

            _timeoutTimer -= Core.FrameTime;

            if (_timeoutTimer <= 0 && !_looping)
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
            _thread.Abort();
            _thread = null;
        }
    }
}