#region Using

using System.Collections;

#endregion

namespace Emotion.Game.Time.Routines
{
    /// <summary>
    /// Run two coroutines together, or wait for two waiters at the same time.
    /// </summary>
    public class CombineWait : IRoutineWaiter
    {
        public bool Finished
        {
            get => _a.Finished && _b.Finished;
        }

        private IRoutineWaiter _a;
        private IRoutineWaiter _b;

        public CombineWait(IEnumerator a, IEnumerator b)
        {
            _a = new Coroutine(a);
            _b = new Coroutine(b);
        }

        public CombineWait(IRoutineWaiter a, IRoutineWaiter b)
        {
            _a = a;
            _b = b;
        }

        public virtual void Update()
        {
            _a.Update();
            _b.Update();
        }
    }

    public class CombineWaitMany : IRoutineWaiter
    {
        public bool Finished { get; protected set; }
        private IRoutineWaiter[] _waiters;

        public CombineWaitMany(params IEnumerator[] routines)
        {
            _waiters = new IRoutineWaiter[routines.Length];
            for (var i = 0; i < _waiters.Length; i++)
            {
                _waiters[i] = new Coroutine(routines[i]);
            }
        }

        public CombineWaitMany(params IRoutineWaiter[] waiters)
        {
            _waiters = waiters;
        }

        public virtual void Update()
        {
            var allDone = true;
            for (var i = 0; i < _waiters.Length; i++)
            {
                IRoutineWaiter waiter = _waiters[i];
                waiter.Update();
                allDone = allDone && waiter.Finished;
            }

            Finished = allDone;
        }
    }
}