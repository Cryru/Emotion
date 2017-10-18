// SoulEngine - https://github.com/Cryru/SoulEngine

using System;

namespace Soul.Engine
{
    public class Timer
    {
        #region Information

        /// <summary>
        /// Whether the timer has finished ticking.
        /// </summary>
        public bool Finished
        {
            get { return _finished; }
        }

        private bool _finished = false;

        /// <summary>
        /// Whether the timer should run.
        /// </summary>
        public bool Running = true;

        #endregion

        #region Internal

        private int _tickRate = 0;
        private int _timer = 0;
        private int _currentTick = 0;
        private int _tickLimit = 0;
        private object _userData;

        #endregion

        #region Events

        /// <summary>
        /// Triggered when the timer ticks.
        /// </summary>
        public Action<Timer, object> OnTick = null;

        /// <summary>
        /// Triggered when the timer finishes.
        /// </summary>
        public Action<Timer, object> OnFinish = null;

        #endregion

        /// <summary>
        /// Create a new timer.
        /// </summary>
        /// <param name="tickRate">How often the timer should tick.</param>
        /// <param name="tickCount">How many times the timer should tick.</param>
        /// <param name="data">Data to send to the events.</param>
        public Timer(int tickRate, int tickCount = -1, object data = null)
        {
            _tickRate = tickRate;
            _tickLimit = tickCount;
            _userData = data;
        }

        /// <summary>
        /// Updates the timer.
        /// </summary>
        internal void Update()
        {
            if (Running)
            {
                _timer += Core.FrameTime;
            }

            ProcessTick();
        }

        /// <summary>
        /// Processes ticking logic.
        /// </summary>
        private void ProcessTick()
        {
            // Check if past the tick rate.
            if (_timer > _tickRate)
            {
                _timer -= _tickRate;
                OnTick?.Invoke(this, _userData);
                _currentTick++;

                // If the timer still has enough time to tick, do so.
                if (_timer > _tickRate) ProcessTick();
            }

            // Check if enough ticks have been done.
            if (_currentTick >= _tickLimit)
            {
                OnFinish?.Invoke(this, _userData);
                _finished = true;
            }
        }

    }
}