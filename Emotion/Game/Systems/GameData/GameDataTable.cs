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

            _objects.Sort(static (x, y) => x.Index - y.Index);
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

        public IReadOnlyList<T> GetCollection<T>() where T : GameDataObject
        {
            return (IReadOnlyList<T>)_objects;
        }
    }
}