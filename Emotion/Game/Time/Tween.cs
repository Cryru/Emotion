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

        protected Func<float, float> _tweenFunc;

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

    public class ProxyTweenTimer<T> : ITimer where T : ITimer
    {
        public float Progress
        {
            get => _tweenFunc(BackingTimer.Progress);
        }

        public bool Finished
        {
            get => BackingTimer.Finished;
        }

        public T BackingTimer { get; protected set; }
        protected Func<float, float> _tweenFunc;

        public ProxyTweenTimer(T backingTimer, TweenMethod method, TweenType type = TweenType.In)
        {
            BackingTimer = backingTimer;
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
            BackingTimer.Update(timePassed);
        }

        public void End()
        {
            BackingTimer.End();
        }

        public void Restart()
        {
            BackingTimer.Restart();
        }

        protected ProxyTweenTimer(T backingTimer)
        {
            BackingTimer = backingTimer;
        }

        public ITimer Clone()
        {
            return new ProxyTweenTimer<T>((T) BackingTimer.Clone())
            {
                _tweenFunc = _tweenFunc
            };
        }
    }
}