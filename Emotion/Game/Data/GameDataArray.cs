#nullable enable

#region Using

using System.Collections;

#endregion

namespace Emotion.Game.Data;


// Protects the game data list and allows enumeration and indexing.
public class GameDataArray<T> : IEnumerable<T>, IList<T> where T : GameDataObject
{
    public static GameDataArray<T> Empty { get; } = new GameDataArray<T>(new List<GameDataObject>());

    private List<GameDataObject> _objects;

    public int Length { get => _objects.Count; }

    public GameDataArray(List<GameDataObject> objects)
    {
        _objects = objects;
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return new GameDataArrayEnum<T>(_objects);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new GameDataArrayEnum<T>(_objects);
    }

    public T this[int key]
    {
        get => (T)_objects[key];
    }

    #region List Interface

    public int Count => _objects.Count;

    public bool IsReadOnly => true;

    T IList<T>.this[int index] { get => (T)_objects[index]; set => throw new NotImplementedException(); }

    public int IndexOf(T item)
    {
        return _objects.IndexOf(item);
    }

    public bool Contains(T item)
    {
        return _objects.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _objects.CopyTo(array, arrayIndex);
    }

    public void Insert(int index, T item)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }

    public void Add(T item)
    {
        throw new NotImplementedException();
    }

    public bool Remove(T item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    #endregion
}
