#nullable enable

#region Using

using System.Linq;
using System.Threading.Tasks;
using Emotion.Editor.EditorHelpers;
using GameDataObjectAsset = Emotion.IO.XMLAsset<Emotion.Game.Data.GameDataObject>;

#endregion

namespace Emotion.Game.Data;

public static partial class GameDataDatabase
{
    public static bool Initialized { get; private set; }

    private static Dictionary<Type, GameDataCache>? _database;

    private const string DATA_OBJECTS_PATH = "Data"; // Assets folder scoped

    public static void Initialize()
    {
        if (Initialized) return;

        _database = new Dictionary<Type, GameDataCache>();

        // Load all game data assets.
        // Direct descendants of the GameDataObject class are considered valid.
        List<Task> loadingTasks = new();
        List<Type>? types = EditorUtility.GetTypesWhichInherit<GameDataObject>(true, true);
        for (var i = 0; i < types.Count; i++)
        {
            Type type = types[i];
            var cache = new GameDataCache(type);
            AssertNotNull(cache);
            _database.Add(type, cache);

            string typeName = type.Name;
            string[] files = Engine.AssetLoader.GetAssetsInFolder($"{DATA_OBJECTS_PATH}/{typeName}");

            for (var j = 0; j < files.Length; j++)
            {
                string file = files[j];
                Task task = GameDataCache.LoadGameDataAssetTask(file, cache);
                loadingTasks.Add(task);
            }
        }
        Task.WaitAll(loadingTasks.ToArray());

        // Create id map for refencing class-data merges.
        foreach (KeyValuePair<Type, GameDataCache> cache in _database)
        {
            cache.Value.RecreateIdMap();
        }

        // Create data entries for class defined items.
        // Class defined items are classes which inherit a class which inherits GameDataObject.
        // That way data definitions can contain code.
        for (var t = 0; t < types.Count; t++)
        {
            Type gameDataType = types[t];
            GameDataCache cache = _database[gameDataType];

            List<Type>? items = EditorUtility.GetTypesWhichInherit(gameDataType, true, true);
            for (int i = 0; i < items.Count; i++)
            {
                Type itemType = items[i];
                string classDefId = itemType.Name;

                if (cache.IdMap.TryGetValue(classDefId, out int idx))
                {
                    GameDataObject existingItem = cache.Objects[idx];
                    if (existingItem.GetType().IsAssignableTo(itemType))
                    {
                        // Data type is saved as this class type - no merge needed.
                        continue;
                    }

                    // Data type existing but is not of class type, create new instance and copy properties.
                    GameDataObject? newItemAsType = EditorUtility.CreateNewObjectOfType(itemType) as GameDataObject;
                    AssertNotNull(newItemAsType);

                    EditorUtility.CopyObjectProperties(existingItem, newItemAsType);
                    newItemAsType.LoadedFromClass = true;
                    cache.Objects[idx] = newItemAsType;
                    continue;
                }

                GameDataObject? newItem = EditorUtility.CreateNewObjectOfType(itemType) as GameDataObject;
                AssertNotNull(newItem);
                newItem.Id = itemType.Name;
                newItem.LoadedFromClass = true;
                cache.Objects.Add(newItem);
            }
        }

        // Rebuild the index map after class defs have been added.
        foreach (KeyValuePair<Type, GameDataCache> cache in _database)
        {
            cache.Value.RecreateIdMap();
        }

        Initialized = true;
    }

    #region Public API

    public static GameDataArray<T>? GetObjectsOfType<T>() where T : GameDataObject
    {
        if (!Initialized) return null;
        AssertNotNull(_database);

        if (_database.TryGetValue(typeof(T), out GameDataCache? cache))
            return cache.GetDataEnum<T>();

        return null;
    }

    public static GameDataArray<GameDataObject>? GetObjectsOfType(Type? type)
    {
        if (!Initialized) return null;
        if (type == null) return null;
        AssertNotNull(_database);

        if (_database.TryGetValue(type, out GameDataCache? cache))
            return cache.GetDataEnum<GameDataObject>();

        return null;
    }

    public static T? GetDataObject<T>(string? name) where T : GameDataObject
    {
        if (!Initialized) return null;
        if (name == null) return null;
        AssertNotNull(_database);

        if (_database.TryGetValue(typeof(T), out GameDataCache? cache))
            return (T?)cache.GetObjectById(name);

        return null;
    }

    public static Type[]? GetGameDataTypes()
    {
        if (!Initialized) return null;
        AssertNotNull(_database);
        return _database.Keys.ToArray();
    }

    #endregion

    public static EditorAdapter GetEditorAdapter()
    {
        return new EditorAdapter();
    }
}