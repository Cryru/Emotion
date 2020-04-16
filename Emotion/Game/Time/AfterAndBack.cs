﻿#region Using

using System;

#endregion

namespace Emotion.Game.Time
{
    public class AfterAndBack : After
    {
        public override float Progress
        {
            get => _timePassed / (InReverse ? _reverseDelay : _delay);
        }

        public bool InReverse { get; protected set; }
        private float _reverseDelay;

        public AfterAndBack(float delay, Action function = null) : base(delay, function)
        {
            _reverseDelay = delay;
        }

        public AfterAndBack(float delay, float reverseDelay, Action function = null) : base(delay, function)
        {
            _reverseDelay = reverseDelay;
        }

        public void GoInReverse()
        {
            Restart();
            InReverse = true;
            _timePassed = _reverseDelay;
        }

        public void GoNormal()
        {
            Restart();
            InReverse = false;
        }

        public override void Update(float timePassed)
        {
            if (Finished) return;

            if (!InReverse)
            {
                base.Update(timePassed);
                return;
            }

            _timePassed -= timePassed;
            if (_timePassed <= 0) End();
        }

        public override void End()
        {
            if (InReverse)
                _timePassed = 0;
            else
                _timePassed = _delay;
            _function?.Invoke();
            Finished = true;
        }
    }
}