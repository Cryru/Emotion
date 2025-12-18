#nullable enable

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Emotion.Standard.Extensions;

public static class DictionaryExtensions
{
    // Matches Dictionary<TKey,TValue> layout
    [StructLayout(LayoutKind.Sequential)]
    private struct DictionaryData<TKey, TValue>
    {
        public int[] buckets;
        public Entry<TKey, TValue>[] entries;
        public int count;
        public int freeList;
        public int freeCount;
        public int version;
        public IEqualityComparer<TKey> comparer;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Entry<TKey, TValue>
    {
        public int hashCode;
        public int next;
        public TKey key;
        public TValue value;
    }

    public static int RemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> dict, Func<TKey, TValue, bool> functor) where TKey : notnull
    {
        // Hopefully the Dictionary layout doesn't change hehe xd
        ref DictionaryData<TKey, TValue> data = ref Unsafe.As<Dictionary<TKey, TValue>, DictionaryData<TKey, TValue>>(ref dict);

        var entries = data.entries;
        int removed = 0;

        for (int i = 0; i < data.count; i++)
        {
            ref Entry<TKey, TValue> entry = ref entries[i];

            if (entry.hashCode >= 0 && functor(entry.key, entry.value))
            {
                entry.hashCode = -1;
                entry.next = data.freeList;
                data.freeList = i;
                removed++;
            }
        }

        data.freeCount += removed;

        return removed;
    }
}
