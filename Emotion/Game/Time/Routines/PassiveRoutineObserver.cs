namespace Emotion.Game.Time.Routines
{
    public class PassiveRoutineObserver : IRoutineWaiter
    {
        private IRoutineWaiter _wrap;

        public PassiveRoutineObserver(IRoutineWaiter wrap)
        {
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