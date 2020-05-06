#region Using

using System;
using Emotion.Common;
using Emotion.Game.Time.Routines;

#endregion

namespace Emotion.Game.Time
{
    public class After : ITimer, IRoutineWaiter
    {
        public virtual float Progress
        {
            get => _timePassed / _delay;
        }

        protected float _timePassed;
        protected float _delay;
        protected Action _function;

        public After(float delay, Action function = null)
        {
            _delay = delay;
            _function = function;
        }

        public virtual void Update(float timePassed)
        {
            if (Finished) return;

            _timePassed += timePassed;
            if (_timePassed >= _delay) End();
        }

        public virtual void End()
        {
            _timePassed = _delay;
            _function?.Invoke();
            Finished = true;
        }

        public virtual void Restart()
        {
            _timePassed = 0;
            Finished = false;
        }

        #region Routine Waiter API

        public bool Finished { get; protected set; }

        public void Update()
        {
            Update(Engine.DeltaTime);
        }

        #endregion

        public override string ToString()
        {
            return $"Delay: {_delay}, Progress: {Math.Round(Progress, 2)}, Finished: {Finished}";
        }
    }
}