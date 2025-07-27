namespace Emotion.Game.Routines
{
    public class PassiveRoutineObserver : IRoutineWaiter
    {
        private IRoutineWaiter _wrap;

        public PassiveRoutineObserver(IRoutineWaiter wrap)
        {
            Assert(wrap != null);
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