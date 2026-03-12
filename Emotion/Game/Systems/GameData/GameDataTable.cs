#nullable enable

using Emotion.Standard.Reflector.Handlers.Interfaces;
using GameDataObjectAsset = Emotion.Core.Systems.IO.XMLAsset<Emotion.Game.Systems.GameData.GameDataObject>;

namespace Emotion.Game.Systems.GameData;

public static partial class GameDatabase
{
    // Database internal class for handling storage of game data objects of a particular type.
    private sealed class GameDataTable
    {
        public IGenericReflectorTypeHandler Handler;
        public Type Type { get; init; }
        public int ObjectCount { get => _objects.Count; }

        private List<GameDataObject> _objects = new();
        private Dictionary<string, int> _idMap = new(StringComparer.OrdinalIgnoreCase);

        public GameDataTable(IGenericReflectorTypeHandler reflectorHandler, Type type)
        {
            Handler = reflectorHandler;
            Type = type;
        }

        #region Loading

        private List<GameDataObjectAsset>? _loadingObjects;

        public void Loading_RegisterAsset(GameDataObjectAsset asset)
        {
            _loadingObjects ??= new List<GameDataObjectAsset>();
            _loadingObjects.Add(asset);
        }

        public IEnumerator Loading_Process()
        {
            if (_loadingObjects == null) yield break;
            foreach (GameDataObjectAsset ass in _loadingObjects)
            {
                yield return ass;
                GameDataObject? content = ass.Content;
                if (content != null)
                    _objects.Add(content);
            }

            _objects.Sort();
            RecreateIdMap();

            _loadingObjects = null;
        }

        #endregion

        #region Editor Functionality

        public string EnsureNonDuplicatedId(string name)
        {
            var counter = 1;
            string originalName = name;
            while (_idMap.ContainsKey(name)) name = originalName + "_" + counter++;
            return name;
        }

        public void RecreateIdMap()
        {
            _idMap.Clear();

            _objects.Sort();
            for (int i = 0; i < _objects.Count; i++)
            {
                GameDataObject obj = _objects[i];
                if (_idMap.ContainsKey(obj.Id))
                {
                    Engine.Log.Warning($"Found duplicate game data id {obj.Id} of type {Type.Name}", MessageSource.GameData);
                    obj.Id = EnsureNonDuplicatedId(obj.Id);
                }

                _idMap.Add(obj.Id, i);
            }
        }

        #endregion

        public GameDataObject? GetObjectById(string id)
        {
            if (!_idMap.TryGetValue(id, out int idx)) return null;
            return _objects[idx];
        }

        public void AddObject(GameDataObject obj)
        {
            obj.Id = EnsureNonDuplicatedId(obj.Id);
            _objects.Add(obj);
            _idMap.TryAdd(obj.Id, _objects.Count - 1);
        }

        public void DeleteObject(GameDataObject obj)
        {
            _objects.Remove(obj);
            _idMap.Remove(obj.Id);
        }

        public void ReplaceObject(GameDataObject obj)
        {
            if (!_idMap.TryGetValue(obj.Id, out int idx)) return;
            _objects[idx] = obj;
        }

        // Reduce allocations by caching the workaround instances
        private Dictionary<Type, object> _covariantListHandlers = new();

        public IReadOnlyList<T> GetCollection<T>() where T : GameDataObject
        {
            // If you've got a better idea...

            Type typ = typeof(T);
            if (typ == typeof(GameDataObject)) return (IReadOnlyList<T>) _objects;

            if (_covariantListHandlers.TryGetValue(typ, out object? val))
                return (IReadOnlyList<T>)val;

            var newList = new CovariantListWorkaround<GameDataObject, T>(_objects)!;
            _covariantListHandlers.Add(typ, newList);
            return newList!;
        }
    }

    // Workaround class for C# lists being invariant
    public class CovariantListWorkaround<T, T2> : IReadOnlyList<T2?>
        where T2 : T
    {
        private List<T> _list;

        public CovariantListWorkaround(List<T> list)
        {
            _list = list;
        }

        public T2? this[int index] => (T2?)_list[index];

        public int Count => _list.Count;

        public IEnumerator<T2?> GetEnumerator()
        {
            foreach (T? item in _list)
            {
                yield return (T2?)item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}