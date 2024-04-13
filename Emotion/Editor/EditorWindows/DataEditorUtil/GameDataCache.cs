#nullable enable

#region Using

using System.Collections;
using System.Threading.Tasks;
using GameDataObjectAsset = Emotion.IO.XMLAsset<Emotion.Editor.EditorWindows.DataEditorUtil.GameDataObject>;

#endregion

namespace Emotion.Editor.EditorWindows.DataEditorUtil;

public static partial class GameDataDatabase
{
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

    // Protects the game data list and allows enumeration and indexing.
    public class GameDataArray<T> : IEnumerable<T> where T : GameDataObject
    {
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
    }

    // Database internal class for handling storage of game data objects of a particular type.
    private sealed class GameDataCache
    {
        public Type Type { get; init; }
        public List<GameDataObject> Objects = new();
        public Dictionary<string, int> IdMap = new(StringComparer.OrdinalIgnoreCase);

        public GameDataCache(Type type)
        {
            Type = type;
        }

        public void RecreateIdMap()
        {
            IdMap.Clear();

            Objects.Sort();
            for (int i = 0; i < Objects.Count; i++)
            {
                GameDataObject obj = Objects[i];
                if (IdMap.ContainsKey(obj.Id))
                {
                    Engine.Log.Warning($"Found duplicate game data id {obj.Id} of type {Type.Name}", MessageSource.GameData);
                    obj.Id = EnsureNonDuplicatedId(obj.Id);
                }

                IdMap.Add(obj.Id, i);
            }
        }

        public string EnsureNonDuplicatedId(string name)
        {
            var counter = 1;
            string originalName = name;
            while (IdMap.ContainsKey(name)) name = originalName + "_" + counter++;
            return name;
        }

        public GameDataObject? GetObjectById(string id)
        {
            if (!IdMap.TryGetValue(id, out int idx)) return null;
            return Objects[idx];
        }

        public static async Task LoadGameDataAssetTask(string fileName, GameDataCache thisAssetCache)
        {
            GameDataObjectAsset? asset = await Engine.AssetLoader.GetAsync<GameDataObjectAsset>(fileName, false);
            if (asset == null || asset.Content == null) return;

            GameDataObject gameDataObjContent = asset.Content;
            gameDataObjContent.LoadedFromFile = asset.Name;

            if (thisAssetCache == null) return;

            lock (thisAssetCache)
            {
                thisAssetCache.Objects.Add(gameDataObjContent);
            }
        }

        public GameDataArray<T> GetDataEnum<T>() where T : GameDataObject
        {
            return new GameDataArray<T>(Objects);
        }
    }
}