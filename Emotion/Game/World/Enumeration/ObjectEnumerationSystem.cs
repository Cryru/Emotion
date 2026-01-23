#nullable enable

namespace Emotion.Game.World.Enumeration;

public class ObjectEnumerationSystem
{
    private List<GameObject> _objects = new List<GameObject>();
    private Queue<GameObject> _objectsToRemove = new Queue<GameObject>();
    private Queue<GameObject> _objectsToAdd = new Queue<GameObject>();
    private int _activeEnumerators = 0;

    private readonly Lock _objectMutationLock = new Lock();

    public ObjectEnumerator GetEnumerator()
    {
        lock (_objectMutationLock)
        {
            _activeEnumerators++;
            return new ObjectEnumerator(this);
        }
    }

    public void OnEnumeratorOver()
    {
        lock (_objectMutationLock)
        {
            _activeEnumerators--;
            TryUpdateObjectList();
        }
    }

    private void TryUpdateObjectList()
    {
        lock (_objectMutationLock)
        {
            if (_activeEnumerators == 0 && (_objectsToRemove.Count != 0 || _objectsToAdd.Count != 0))
            {
                while (_objectsToRemove.TryDequeue(out GameObject? obj))
                {
                    _objects.Remove(obj);
                    obj.Done();
                }

                while (_objectsToAdd.TryDequeue(out GameObject? obj))
                {
                    _objects.Add(obj);
                }
            }
        }
    }

    public void AssertNoActiveEnumerations()
    {
        Assert(_activeEnumerators == 0);
    }

    public void AddObject(GameObject obj)
    {
        lock (_objectMutationLock)
        {
            _objectsToAdd.Enqueue(obj);
        }
        TryUpdateObjectList();
    }

    public void RemoveObject(GameObject obj)
    {
        lock (_objectMutationLock)
        {
            _objectsToRemove.Enqueue(obj);
        }
        TryUpdateObjectList();
    }

    public ref struct ObjectEnumerator
    {
        public readonly GameObject Current => _system._objects[_idx];

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