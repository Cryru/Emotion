// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Engine;

#endregion

namespace Emotion
{
    public static class Starter
    {
        /// <summary>
        /// Creates and returns an Emotion context.
        /// </summary>
        /// <param name="config">A function to apply settings in.</param>
        /// <returns>An Emotion context.</returns>
        public static Context GetEmotionContext(Action<Settings> config = null)
        {
            // Apply settings.
            Settings initial = new Settings();

            // Setup thread manager.
            ThreadManager.BindThread();

            config?.Invoke(initial);

            return new Context(initial);
        }
    }
}