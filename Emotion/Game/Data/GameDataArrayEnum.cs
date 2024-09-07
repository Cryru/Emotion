#nullable enable

#region Using

using System.Collections;

#endregion

namespace Emotion.Game.Data;

public struct GameDataArrayEnum<T> : IEnumerator<T> where T : GameDataObject
{
    private int _currentIndex = -1;
    private List<GameDataObject> _objects;

    public T Current => (T)_objects[_currentIndex];

    object IEnumerator.Current => Current;

    public GameDataArrayEnum(List<GameDataObject> objects)
    {
        _objects = objects;
    }

    public void Dispose()
    {
        _objects = null!;
    }

    public bool MoveNext()
    {
        _currentIndex++;
        return _currentIndex < _objects.Count;
    }

    public void Reset()
    {
        _currentIndex = 0;
    }
}