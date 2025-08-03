#region Using

#endregion

namespace StbTrueTypeSharp
{
    internal static class MemoryStats
    {
        private static int _allocations;

        public static int Allocations
        {
            get => _allocations;
        }

        internal static void Allocated()
        {
            Interlocked.Increment(ref _allocations);
        }

        internal static void Freed()
        {
            Interlocked.Decrement(ref _allocations);
        }
    }
}