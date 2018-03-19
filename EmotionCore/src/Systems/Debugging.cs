// Emotion - https://github.com/Cryru/Emotion

#if DEBUG

#region Using

using System;

#endregion

namespace Emotion.Systems
{
    public static class Debugging
    {
        /// <summary>
        /// Add a message to the debug log.
        /// </summary>
        /// <param name="message">The message to add.</param>
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
#endif