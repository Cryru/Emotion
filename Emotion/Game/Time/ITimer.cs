namespace Emotion.Game.Time
{
    public interface ITimer
    {
        /// <summary>
        /// The timer progress from 0 to 1.
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// Whether the timer is finished.
        /// </summary>
        bool Finished { get; }

        /// <summary>
        /// Advance time for the timer.
        /// </summary>
        /// <param name="timePassed">Time that has passed.</param>
        void Update(float timePassed);

        /// <summary>
        /// Force end the timer. This will advance it to its end.
        /// </summary>
        void End();

        /// <summary>
        /// Restart the timer.
        /// </summary>
        void Restart();

        /// <summary>
        /// Create a copy of this timer. Doesn't include the internal state such as the current progress or loop count.
        /// </summary>
        ITimer Clone();
    }
}