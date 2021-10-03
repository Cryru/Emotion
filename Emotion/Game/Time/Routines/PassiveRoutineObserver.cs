#region Using

using System.Diagnostics;

#endregion

namespace Emotion.Game.Time.Routines
{
    public class PassiveRoutineObserver : IRoutineWaiter
    {
        private IRoutineWaiter _wrap;

        public PassiveRoutineObserver(IRoutineWaiter wrap)
        {
            Debug.Assert(wrap != null);
            _wrap = wrap;
        }

        public bool Finished
        {
            get => _wrap.Finished;
        }

        public void Update()
        {
            // no-op
        }
    }
}