#region Using

using System;

#endregion

namespace Emotion.Game.Time
{
    public class AfterAndBack : After
    {
        public override float Progress
        {
            get => _timePassed / (InReverse ? ReverseDelay : Delay);
        }

        public bool InReverse { get; protected set; }
        public float ReverseDelay { get; set; }

        public AfterAndBack(float delay, Action function = null) : base(delay, function)
        {
            ReverseDelay = delay;
        }

        public AfterAndBack(float delay, float reverseDelay, Action function = null) : base(delay, function)
        {
            ReverseDelay = reverseDelay;
        }

        /// <summary>
        /// Restart the timer, and go in reverse.
        /// </summary>
        public void GoInReverse()
        {
            Restart();
            InReverse = true;
            _timePassed = ReverseDelay;
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
                _timePassed = Delay;
            _function?.Invoke();
            Finished = true;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, InReverse: {InReverse}";
        }
    }
}