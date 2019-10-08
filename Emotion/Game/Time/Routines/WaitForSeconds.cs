using Emotion.Common;

namespace Emotion.Game.Time.Routines
{
    /// <inheritdoc />
    public sealed class WaitForSeconds : IRoutineWaiter
    {
        /// <summary>
        /// The time to wait for.
        /// </summary>
        public float Time { get; private set; }

        /// <inheritdoc />
        public bool Finished
        {
            get => _timer >= Time;
        }

        private float _timer;

        /// <summary>
        /// Create a new time waiter.
        /// </summary>
        /// <param name="seconds">The number of seconds to wait for.</param>
        public WaitForSeconds(float seconds)
        {
            Time = seconds * 1000;
        }

        /// <inheritdoc />
        public void Update()
        {
            _timer += Engine.DeltaTime;
        }
    }
}