#region Using

using System;

#endregion

namespace Adfectus.Game.Time
{
    internal class After : TimerInstance
    {
        private float _timePassed;

        private float _delay;
        private Action _function;
        public object Tag { get; private set; }

        internal After(float delay, Action function)
        {
            _delay = delay;
            _function = function;
        }

        #region Inheritance

        internal override void TimerLogic(float timePassed)
        {
            _timePassed += timePassed;

            if (_timePassed >= _delay) _function?.Invoke();
        }

        protected override void End()
        {
            _function = null;
            Tag = null;
        }

        #endregion
    }
}