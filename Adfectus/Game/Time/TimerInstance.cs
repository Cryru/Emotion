namespace Adfectus.Game.Time
{
    internal abstract class TimerInstance
    {
        /// <summary>
        /// Whether the timer is dead and can be disposed of.
        /// </summary>
        public bool Dead { get; private set; }

        /// <summary>
        /// Advance time for the timer.
        /// </summary>
        /// <param name="timePassed">Time that has passed.</param>
        public void AdvanceTime(float timePassed)
        {
            // Run timer logic if not dead.
            if (!Dead) TimerLogic(timePassed);
        }

        /// <summary>
        /// Kill the timer instance.
        /// </summary>
        public void Kill()
        {
            // Check if already killed.
            if (Dead) return;

            Dead = true;
            End();
        }

        internal abstract void TimerLogic(float timePassed);
        protected abstract void End();
    }
}