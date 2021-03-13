#region Using

using System;

#endregion

namespace Emotion.Game.Time
{
    public class Every : ITimer
    {
        public bool Finished
        {
            get => _currentCount >= _count && _count > 0;
        }

        public float Progress
        {
            get => _timePassed / _delay;
        }

        public float CountProgress
        {
            get => _count < 0 ? 0 : _currentCount / _count;
        }

        private float _timePassed;
        private int _currentCount;

        private readonly float _delay;
        private readonly Action _function;
        private readonly int _count;
        private readonly Action _after;

        public Every(float delay, Action function, int count = -1, Action after = null)
        {
            _delay = delay;
            _function = function;
            _count = count;
            _after = after;
        }

        public void Update(float timePassed)
        {
            _timePassed += timePassed;

            while (_timePassed >= _delay)
            {
                _timePassed -= _delay;

                CallFunc();

                // Check if the current count is sufficient.
                if (Finished) _after?.Invoke();
            }
        }

        public void End()
        {
            if (_count > 0)
                for (int i = _currentCount; i <= _count; i++)
                {
                    CallFunc();
                }

            _after?.Invoke();
        }

        public void Restart()
        {
            _currentCount = 0;
            _timePassed = 0;
        }

        public ITimer Clone()
        {
            return new Every(_delay, _function, _count, _after);
        }

        /// <summary>
        /// Increment count and call callback.
        /// </summary>
        private void CallFunc()
        {
            _function?.Invoke();
            _currentCount++;
        }
    }
}