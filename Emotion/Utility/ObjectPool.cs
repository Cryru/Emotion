#region Using

using System;
using System.Collections.Concurrent;

#endregion

#nullable enable

namespace Emotion.Utility
{
    /// <summary>
    /// A pool of objects. Thread safe.
    /// </summary>
    public class ObjectPool<T> where T : new()
    {
        private readonly ConcurrentBag<T> _objects = new ConcurrentBag<T>();
        private Func<T> _createMethod;
        private Action<T>? _resetMethod;

        public ObjectPool() : this(() => new T())
        {
        }

        public ObjectPool(Action<T>? resetMethod = null, int preCreate = -1) : this(() => new T(), resetMethod, preCreate)
        {
        }

        public ObjectPool(Func<T> createMethod, Action<T>? resetMethod = null, int createInAdvance = -1)
        {
            _createMethod = createMethod;
            _resetMethod = resetMethod;

            for (var i = 0; i < createInAdvance; i++)
            {
                _objects.Add(_createMethod());
            }
        }

        public T Get()
        {
            return _objects.TryTake(out T? item) ? item : _createMethod();
        }

        public void Return(T item)
        {
#if DEBUG_OBJECTPOOL
            foreach (T obj in _objects)
            {
                Debug.Assert(!obj.Equals(item));
            }
#endif
            _resetMethod?.Invoke(item);
            _objects.Add(item);
        }
    }
}