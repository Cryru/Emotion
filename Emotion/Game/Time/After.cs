#region Using

using System;
using Emotion.Common;
using Emotion.Game.Time.Routines;

#endregion

namespace Emotion.Game.Time
{
    public class After : ITimer, IRoutineWaiter
    {
        public float Progress
        {
            get => _timePassed / _delay;
        }

        private float _timePassed;
        private float _delay;
        private Action _function;

        public After(float delay, Action function = null)
        {
            _delay = delay;
            _function = function;
        }

        public void Update(float timePassed)
        {
            if (Finished) return;

            _timePassed += timePassed;
            if (_timePassed >= _delay) End();
        }

        public void End()
        {
            _timePassed = _delay;
            _function?.Invoke();
            Finished = true;
        }

        public void Restart()
        {
            _timePassed = 0;
            Finished = false;
        }

        #region Routine Waiter API

        public bool Finished { get; private set; }

        public void Update()
        {
            Update(Engine.DeltaTime);
        }

        #endregion
    }
}