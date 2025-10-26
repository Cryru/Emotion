#nullable enable

namespace Emotion.Game.World.Enumeration;

public class ObjectEnumerationSystem
{
    private List<GameObject> _objects = new List<GameObject>();
    private List<GameObject> _objectsToRemove = new List<GameObject>();
    private int _activeEnumerators = 0;

    private readonly Lock _objectDeletionLock = new Lock();

    public ObjectEnumerator GetEnumerator()
    {
        Interlocked.Increment(ref _activeEnumerators);
        return new ObjectEnumerator(this);
    }

    public void OnEnumeratorOver()
    {
        if (Interlocked.Decrement(ref _activeEnumerators) == 0 && _objectsToRemove.Count > 0)
        {
            lock (_objectDeletionLock)
            {
                foreach (GameObject obj in _objectsToRemove)
                {
                    _objects.Remove(obj);
                    obj.Done();
                }
                _objectsToRemove.Clear();
            }
        }
    }

    public void AddObject(GameObject obj)
    {
        lock (_objectDeletionLock)
        {
            _objects.Add(obj);
        }
    }

    public void RemoveObject(GameObject obj)
    {
        lock (_objectDeletionLock)
        {
            _objectsToRemove.Add(obj);
        }
    }

    public struct ObjectEnumerator : IEnumerator<GameObject>
    {
        public readonly GameObject Current => _system._objects[_idx];
        readonly object IEnumerator.Current => Current;

        private ObjectEnumerationSystem _system;
        private int _idx;

        public ObjectEnumerator(ObjectEnumerationSystem system)
        {
            _system = system;
            Reset();
        }

        public void Dispose()
        {
            _system.OnEnumeratorOver();
        }

        public bool MoveNext()
        {
            _idx++;
            return _system._objects.Count > _idx;
        }

        public void Reset()
        {
            _idx = -1;
        }

        public ObjectEnumerator GetEnumerator() => this;
    }
}