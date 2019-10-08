#region Using

using System.Collections.Generic;

#endregion

namespace Emotion.Graphics.Command
{
    public abstract class CommandRecycler
    {
        public abstract object GetObject();
        public abstract void Reset();
        public abstract void Dispose();
    }

    public class CommandRecycler<T> : CommandRecycler where T : RecyclableCommand, new()
    {
        public int CachePointer;
        public List<T> Cache = new List<T>();

        public T Get()
        {
            if (CachePointer == Cache.Count)
            {
                var t = new T();
                Cache.Add(t);
                CachePointer++;
                return t;
            }

            T recycledInstance = Cache[CachePointer++];
            recycledInstance.Recycle();
            return recycledInstance;
        }

        public override object GetObject()
        {
            return Get();
        }

        public override void Reset()
        {
            CachePointer = 0;
        }

        public override void Dispose()
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Cache.Count; i++)
            {
                Cache[i].Dispose();
            }
        }
    }
}