#nullable enable



#nullable enable

namespace Emotion.Standard.Extensions;

public static class DictionaryExtensions
{
    public static void RemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> dict, Func<TKey, TValue, bool> functor) where TKey : notnull
    {
        List<TKey> keysToRemove = new List<TKey>();
        foreach (var item in dict)
        {
            bool remove = functor(item.Key, item.Value);
            if (remove) keysToRemove.Add(item.Key);
        }

        for (int i = 0; i < keysToRemove.Count; i++)
        {
            TKey key = keysToRemove[i];
            dict.Remove(key);
        }
    }
}
