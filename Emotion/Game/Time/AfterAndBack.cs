#region Using

using System;
using System.Runtime.CompilerServices;

#endregion

namespace Emotion.Game.Time
{
    public class AfterAndBack : After
    {
        public override float Progress
        {
            get => GetProgress();
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetProgress()
        {
            return _timePassed / (InReverse ? ReverseDelay : Delay);
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

        /// <summary>
        /// Restart the timer, and go the right way.
        /// </summary>
        public void GoNormal()
        {
            Restart();
            InReverse = false;
        }

        /// <summary>
        /// Go in the opposite of the current direction (or the direction last finished).
        /// </summary>
        public void GoInOpposite()
        {
            if (InReverse)
                GoNormal();
            else
                GoInReverse();
        }

        public override void Update(float timePassed)
        {
            if (Finished) return;

            if (InReverse)
            {
                if (GetProgress() == 0.0f)
                {
                    End();
                    return;
                }

                _timePassed -= timePassed;
                if (_timePassed < 0.0f) _timePassed = 0;
            }
            else
            {
                base.Update(timePassed);
            }
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