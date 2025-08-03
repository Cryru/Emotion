#nullable enable

using System.Collections.Concurrent;

namespace Emotion.Standard.DataStructures;

public class LazyConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, Lazy<TValue>>
{
    public Lazy<TValue> GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        Lazy<TValue> lazyResult = GetOrAdd(key, k => new Lazy<TValue>(() => valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication));
        return lazyResult;
    }

    public TValue GetOrAddValue(TKey key, Func<TKey, TValue> valueFactory)
    {
        Lazy<TValue> lazyResult = GetOrAdd(key, k => new Lazy<TValue>(() => valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication));
        return lazyResult.Value;
    }
}