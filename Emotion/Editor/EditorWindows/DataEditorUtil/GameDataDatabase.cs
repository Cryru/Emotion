#nullable enable

#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Xml.Linq;
using Emotion.Editor.EditorHelpers;
using GameDataObjectAsset = Emotion.IO.XMLAsset<Emotion.Editor.EditorWindows.DataEditorUtil.GameDataObject>;

#endregion

namespace Emotion.Editor.EditorWindows.DataEditorUtil;

public static partial class GameDataDatabase
{
    public static bool Initialized { get; private set; }

    private static Dictionary<Type, GameDataCache>? _database;

    private const string DATA_OBJECTS_PATH = "Data"; // Assets folder scoped

    public static void Initialize()
    {
        if (Initialized) return;

        _database = new Dictionary<Type, GameDataCache>();

        List<Task<GameDataObjectAsset?>> loadingTasks = new();
        List<Type>? types = EditorUtility.GetTypesWhichInherit<GameDataObject>();
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
                var task = Engine.AssetLoader.GetAsync<GameDataObjectAsset>(file, false);
                loadingTasks.Add(task);

                task.ContinueWith(GameDataCache.AssetLoadTaskCallback, cache);
            }
        }
        Task.WaitAll(loadingTasks.ToArray());

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