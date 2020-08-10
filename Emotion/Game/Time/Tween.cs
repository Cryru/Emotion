#region Using

using System;
using Emotion.Game.Time.Timers;
using Emotion.Game.Time.Tweening;

#endregion

namespace Emotion.Game.Time
{
    public class TweenTimer<TTimer> : ITimer where TTimer : ITimer
    {
        public bool Finished
        {
            get => InnerTimer.Finished;
        }

        public float Progress
        {
            get => _tweenFunc(InnerTimer.Progress);
        }

        public TTimer InnerTimer { get; }
        private Func<float, float> _tweenFunc;

        public TweenTimer(TTimer innerTimer, TweenMethod method, TweenType type = TweenType.In)
        {
            InnerTimer = innerTimer;

            _tweenFunc = type switch
            {
                TweenType.In => Tween.In(method),
                TweenType.Out => Tween.Out(method),
                TweenType.InOut => Tween.InOut(method),
                TweenType.OutIn => Tween.OutIn(method),
                _ => _tweenFunc
            };
        }

        public void Update(float timePassed)
        {
            InnerTimer.Update(timePassed);
        }

        public void End()
        {
            InnerTimer.End();
        }

        public void Restart()
        {
            InnerTimer.Restart();
        }
    }
}