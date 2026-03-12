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

    private const string ASSETS_DATA_FOLDER = "DataXML";
    private const string CLASS_DATA_FOLDER = "DataClasses";

    internal static IEnumerator InitializeRoutine()
    {
        Assert(!Initialized);
        if (Initialized) yield break;

        IGenericReflectorTypeHandler[] dataTypes = ReflectorEngine.GetDescendantsOf<GameDataObject>(true);
        foreach (IGenericReflectorTypeHandler handler in dataTypes)
        {
            Type type = handler.Type;
            GameDataTable table = new GameDataTable(handler, type);
            _database.Add(type, table);

            string typeName = type.Name;
            foreach (string file in Engine.AssetLoader.ForEachAssetInFolder($"{ASSETS_DATA_FOLDER}/{typeName}"))
            {
                if (file.Contains(".backup")) continue;
                GameDataObjectAsset obj = Engine.AssetLoader.Get<GameDataObjectAsset>(file, noCache: true);
                table.Loading_RegisterAsset(obj);
            }
        }

        foreach ((Type typ, GameDataTable db) in _database)
        {
            yield return db.Loading_Process();
        }

        // Associate with classes
        foreach (IGenericReflectorTypeHandler handler in dataTypes)
        {
            Type type = handler.Type;
            _database.TryGetValue(type, out GameDataTable? typeTable);
            AssertNotNull(typeTable);

            IGenericReflectorTypeHandler[] dataClasses = ReflectorEngine.GetDescendantsOf(type, true);

            foreach (IGenericReflectorTypeHandler dataTypeHandler in dataClasses)
            {
                Type dataType = dataTypeHandler.Type;
                string dataId = dataType.Name;

                GameDataObject? obj = typeTable.GetObjectById(dataId);
                if (obj != null)
                {
                    // Check if the existing object is of the defined class
                    Type objType = obj.GetType();
                    if (objType != dataType)
                    {
                        Engine.Log.ONE_Trace(MessageSource.GameData, $"Game data {dataId} is of type {objType.Name} rather than its class - {dataType.Name}. Upgrading.");
                        object? objOfType = dataTypeHandler.CreateNew();
                        if (objOfType is GameDataObject objOfTypeAsData)
                        {
                            ReflectorEngine.CopyProperties(obj, objOfTypeAsData);
                            Assert(obj.Id == objOfTypeAsData.Id);
                            typeTable.ReplaceObject(objOfTypeAsData);
                        }
                    }
                }
                else if (dataTypeHandler.CanCreateNew()) // Class only defined - create data entry if class can be initialized
                {
                    object? newObj = dataTypeHandler.CreateNew();
                    if (newObj is GameDataObject newDataObj)
                    {
                        newDataObj.Id = dataId;
                        newDataObj.Index = typeTable.ObjectCount;
                        typeTable.AddObject(newDataObj);

                        Engine.Log.ONE_Trace(MessageSource.GameData, $"Found game data {dataId} only as a class. Adding to list.");
                    }
                }
            }

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