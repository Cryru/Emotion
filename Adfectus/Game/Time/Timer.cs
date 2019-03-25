#region Using

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Adfectus.Game.Time
{
    public class Timer : IDisposable
    {
        /// <summary>
        /// The number of currently active timers.
        /// </summary>
        public int ActiveTimers
        {
            get => Timers.Count;
        }

        /// <summary>
        /// A list of running timers.
        /// </summary>
        internal Dictionary<string, TimerInstance> Timers;

        public Timer()
        {
            Timers = new Dictionary<string, TimerInstance>();
        }

        /// <summary>
        /// Advance time for all timers.
        /// </summary>
        /// <param name="time">The amount of time passed since the last advancement. Should be consistent with timer delays.</param>
        public void AdvanceTime(float time)
        {
            // Update all running timers.
            foreach (KeyValuePair<string, TimerInstance> timer in Timers)
            {
                // Check if exists and alive.
                if (timer.Value == null || timer.Value.Dead) continue;

                timer.Value.TimerLogic(time);
            }

            // Clean dead.
            KeyValuePair<string, TimerInstance>[] deadTimers = Timers.Where(x => x.Value.Dead).ToArray();
            foreach (KeyValuePair<string, TimerInstance> timer in deadTimers)
            {
                Timers.Remove(timer.Key);
            }
        }

        /// <summary>
        /// Stops running the timer with the specified tag.
        /// </summary>
        /// <param name="tag">The tag of the timer to remove.</param>
        public void Cancel(string tag)
        {
            Timers[tag].Kill();
        }

        #region Timers

        /// <summary>
        /// Executes a function after a set delay.
        /// </summary>
        /// <param name="delay">The delay before executing the function.</param>
        /// <param name="function">The function to execute.</param>
        /// <param name="tag">An optional tag for differentiating this timer.</param>
        /// <returns>The tag of the registered timer.</returns>
        public string After(float delay, Action function, string tag = null)
        {
            // Check for empty id.
            if (string.IsNullOrEmpty(tag)) tag = GenerateId();

            if (Timers.ContainsKey(tag))
            {
                Cancel(tag);
                Timers.Remove(tag);
            }

            // Generate instance and add it.
            TimerInstance newInstance = new After(delay, function);
            Timers.Add(tag, newInstance);

            return tag;
        }

        /// <summary>
        /// Executes a function on a set interval.
        /// </summary>
        /// <param name="delay">The delay or interval between each function execution.</param>
        /// <param name="function">The function to execute.</param>
        /// <param name="count">The amount of times the function will be called. By default it will run forever.</param>
        /// <param name="after">The function to execute after the timer has finished.</param>
        /// <param name="tag">An optional tag for differentiating this timer.</param>
        /// <returns>The tag of the registered timer.</returns>
        public string Every(float delay, Action function, int count = -1, Action after = null, string tag = null)
        {
            // Check for empty id.
            if (string.IsNullOrEmpty(tag)) tag = GenerateId();

            if (Timers.ContainsKey(tag))
            {
                Cancel(tag);
                Timers.Remove(tag);
            }

            // Generate instance and add it.
            TimerInstance newInstance = new Every(delay, function, count, after);
            Timers.Add(tag, newInstance);

            return tag;
        }

        /// <summary>
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="objectTarget"></param>
        /// <param name="objectTween"></param>
        /// <param name="type"></param>
        /// <param name="method"></param>
        /// <param name="after"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public string Tween(float duration, object objectTarget, object objectTween, TweenType type, TweenMethod method, Action after = null, string tag = null)
        {
            // Check for empty id.
            if (string.IsNullOrEmpty(tag)) tag = GenerateId();

            if (Timers.ContainsKey(tag))
            {
                Cancel(tag);
                Timers.Remove(tag);
            }

            // Generate instance and add it.
            TimerInstance newInstance = new Tween(duration, ref objectTarget, ref objectTween, type, method, after);
            Timers.Add(tag, newInstance);

            return tag;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Generate a unique id for timers without a specified one.
        /// </summary>
        /// <returns>A unique id.</returns>
        private string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }

        #endregion

        /// <summary>
        /// Destroy the timer object and all its timers.
        /// </summary>
        public void Dispose()
        {
            foreach (KeyValuePair<string, TimerInstance> timer in Timers)
            {
                Cancel(timer.Key);
            }

            Timers.Clear();
            Timers = null;
        }
    }
}