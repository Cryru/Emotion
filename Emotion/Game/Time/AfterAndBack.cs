#region Using

using System;

#endregion

namespace Emotion.Game.Time
{
    public class AfterAndBack : After
    {
        public bool InReverse { get; protected set; }

        public AfterAndBack(float delay, Action function = null) : base(delay, function)
        {
        }

        public void GoInReverse()
        {
            Restart();
            InReverse = true;
            _timePassed = _delay;
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
            {
                _timePassed = 0;
            }
            else
            {
                _timePassed = _delay;
            }
            _function?.Invoke();
            Finished = true;
        }
    }
}