#nullable enable

#region Using

using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using GameDataObjectAsset = Emotion.Core.Systems.IO.XMLAsset<Emotion.Game.Systems.GameData.GameDataObject>;

#endregion

namespace Emotion.Game.Systems.GameData;

public static partial class GameDatabase
{
    public static bool Initialized { get; private set; }

    private static Dictionary<Type, GameDataTable> _database = new();

    internal static IEnumerator InitializeRoutine()
    {
        Assert(!Initialized);
        if (Initialized) yield break;

        IGenericReflectorTypeHandler[] dataTypes = ReflectorEngine.GetDescendantsOf<GameDataObject>(true);
        foreach (IGenericReflectorTypeHandler handler in dataTypes)
        {
            Type type = handler.Type;
            GameDataTable table = new GameDataTable(type);
            _database.Add(type, table);

            IGenericReflectorTypeHandler[] dataDefs = ReflectorEngine.GetDescendantsOf(type, false);
            foreach (IGenericReflectorTypeHandler dataTypeHandler in dataDefs)
            {
                Type dataType = dataTypeHandler.Type;
                string dataId = dataType.Name;

                GameDataObject? obj = table.GetObjectById(dataId);
                Assert(obj == null);

                object? newObj = dataTypeHandler.CreateNew();
                if (newObj is GameDataObject newDataObj)
                {
                    table.AddObject(newDataObj);
                }
            }
        }

        // Finalize loading
        foreach (IGenericReflectorTypeHandler handler in dataTypes)
        {
            Type type = handler.Type;
            _database.TryGetValue(type, out GameDataTable? typeTable);
            AssertNotNull(typeTable);
            Engine.Log.ONE_Info(MessageSource.GameData, $"Loaded {typeTable.ObjectCount} {type.Name}Defs");
        }

        Initialized = true;
    }

    #region Public API

    public static IReadOnlyList<GameDataObject> GetObjectsOfType(Type typ)
    {
        _database.TryGetValue(typ, out GameDataTable? table);
        if (table == null)
            return Array.Empty<GameDataObject>();
        return table.GetCollection<GameDataObject>();
    }

    public static IReadOnlyList<T> GetObjectsOfType<T>()
        where T : GameDataObject
    {
        Type typ = typeof(T);
        _database.TryGetValue(typ, out GameDataTable? table);
        if (table == null)
            return Array.Empty<T>();
        return table.GetCollection<T>();
    }

    public static GameDataObject? GetObject(Type typ, string? name)
    {
        if (name == null) return null;

        _database.TryGetValue(typ, out GameDataTable? table);
        if (table == null) return null;
        return table.GetObjectById(name);
    }

    public static T? GetObject<T>(string? name)
        where T : GameDataObject
    {
        Type? dataType = ReflectorEngine.WalkUpUntilDirectDescendant(typeof(T), typeof(GameDataObject));
        AssertNotNull(dataType);
        if (dataType == null) return null;

        return (T?)GetObject(dataType, name);
    }

    #endregion
}