#nullable enable

#region Using

using System.Collections;

#endregion

namespace Emotion.Game.Data;


// Protects the game data list and allows enumeration and indexing.
public class GameDataArray<T> : IEnumerable<T> where T : GameDataObject
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
}