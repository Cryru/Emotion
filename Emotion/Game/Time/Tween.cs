#region Using

using System;
using Emotion.Game.Time.Timers;
using Emotion.Game.Time.Tweening;

#endregion

namespace Emotion.Game.Time
{
    public class TweenTimer : After
    {
        public override float Progress
        {
            get => _tweenFunc(base.Progress);
        }

        private Func<float, float> _tweenFunc;

        public TweenTimer(int time, TweenMethod method, TweenType type = TweenType.In) : base(time)
        {
            _tweenFunc = type switch
            {
                TweenType.In => Tween.In(method),
                TweenType.Out => Tween.Out(method),
                TweenType.InOut => Tween.InOut(method),
                TweenType.OutIn => Tween.OutIn(method),
                _ => _tweenFunc
            };
        }
    }
}