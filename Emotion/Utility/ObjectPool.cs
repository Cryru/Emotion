#region Using

using System.Collections.Concurrent;

#endregion

#nullable enable

namespace Emotion.Utility;

/// <summary>
/// Object pool where you must manually define the create method.
/// Thread safe.
/// </summary>
public class ObjectPoolManual<T>
{
    private readonly ConcurrentBag<T> _objects = new ConcurrentBag<T>();
    private Func<T> _createMethod;
    private Action<T>? _resetMethod;

    public ObjectPoolManual(Func<T> createMethod, Action<T>? resetMethod = null, int createInAdvance = -1)
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

/// <summary>
/// A pool of objects. Thread safe.
/// </summary>
public class ObjectPool<T> : ObjectPoolManual<T>
    where T : new()
{
    public ObjectPool() : base(() => new T())
    {
    }

    public ObjectPool(Action<T>? resetMethod = null, int preCreate = -1) : base(() => new T(), resetMethod, preCreate)
    {
    }

    public ObjectPool(Func<T> createMethod, Action<T>? resetMethod = null, int createInAdvance = -1) : base(createMethod, resetMethod, createInAdvance)
    {
    }
}