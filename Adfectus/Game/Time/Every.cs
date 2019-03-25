#region Using

using System;

#endregion

namespace Adfectus.Game.Time
{
    internal class Every : TimerInstance
    {
        private float _timePassed;
        private int _currentCount;

        private float _delay;
        private Action _function;
        private int _count;
        private Action _after;

        internal Every(float delay, Action function, int count = -1, Action after = null)
        {
            _delay = delay;
            _function = function;
            _count = count;
            _after = after;
        }

        #region Inheritance

        internal override void TimerLogic(float timePassed)
        {
            _timePassed += timePassed;

            while (timePassed >= _delay)
            {
                timePassed -= _delay;

                // Increment count and call function.
                _function?.Invoke();
                _currentCount++;

                // Check if the current count is sufficient.
                if (_currentCount >= _count && _count > 0)
                {
                    // Invoke ending logic.
                    _after?.Invoke();
                    Kill();
                }
            }
        }

        protected override void End()
        {
            _function = null;
            _after = null;
        }

        #endregion
    }
}