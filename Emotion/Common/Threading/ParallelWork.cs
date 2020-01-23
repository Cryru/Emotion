#region Using

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

#endregion

namespace Emotion.Common.Threading
{
    public class ParallelWork
    {
        /// <summary>
        /// For loops which run fast we batch them into groups.
        /// </summary>
        /// <param name="arraySize">The total size of the array.</param>
        /// <param name="function">
        /// The function to run. The two params you get are
        /// the start (inclusive) and end (exclusive) of the section that should be processed
        /// </param>
        /// <returns>A task which completes when the whole operation completes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task FastLoops(int arraySize, Action<int, int> function)
        {
            // Early out
            if (arraySize <= Environment.ProcessorCount)
            {
                return Task.Run(() => function(0, arraySize));
            }

            var tasks = new Task[Environment.ProcessorCount];
            int perTask = arraySize / tasks.Length;
            int leftOver = arraySize - perTask * tasks.Length;
            for (var t = 0; t < tasks.Length; t++)
            {
                int arrayStart = perTask * t;
                int arrayEnd = arrayStart + perTask;

                // Add the left over amount to the last task.
                if (t == tasks.Length - 1) arrayEnd += leftOver;

                tasks[t] = new Task(() => function(arrayStart, arrayEnd), TaskCreationOptions.LongRunning);
                tasks[t].Start();
            }

            return Task.WhenAll(tasks);
        }
    }
}