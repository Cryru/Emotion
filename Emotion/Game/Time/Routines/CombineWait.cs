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

        public void Update()
        {
            _a.Update();
            _b.Update();
        }
    }
}