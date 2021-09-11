#region Using

using System;
using System.Collections.Concurrent;
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
            if (arraySize == 0) return Task.CompletedTask;
            OrderablePartitioner<Tuple<int, int>> arrayPartitions = Partitioner.Create(0, arraySize);
            Parallel.ForEach(arrayPartitions, (range, loopState) =>
            {
                (int start, int end) = range;
                function(start, end);
            });
            return Task.CompletedTask;
        }
    }
}