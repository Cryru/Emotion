#nullable enable

#region Using

using System.Linq;
using System.Threading.Tasks;
using Emotion.Editor.EditorHelpers;
using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.Reflector;
using static Emotion.Game.Data.GameDataDatabase;
using GameDataObjectAsset = Emotion.IO.XMLAsset<Emotion.Game.Data.GameDataObject>;

#endregion

namespace Emotion.Game.Data;

public static partial class GameDataDatabase
{
    public static bool Initialized { get; private set; }

    private static Dictionary<Type, GameDataObject[]> _definedData = new();

    public static void Initialize()
    {
        Assert(!Initialized);
        if (Initialized) return;

        Type[] gameDataTypes = ReflectorEngine.GetTypesDescendedFrom(typeof(GameDataObject), true);
        foreach (Type typ in gameDataTypes)
        {
            ComplexTypeHandlerMember? member = EditorAdapter.GetStaticAllDefinitionsMember(typ);
            if (member == null) continue;

            member.GetValueFromComplexObject(new object(), out object? val); // This is how you read from a static member lol
            GameDataObject[]? array = val as GameDataObject[];
            AssertNotNull(array);

            _definedData.Add(typ, array);
        }

        Initialized = true;
    }

    #region Public API

    public static GameDataObject[] GetObjectsOfType(Type typ)
    {
        _definedData.TryGetValue(typ, out GameDataObject[]? definitions);
        if (definitions == null)
        {
            // We initialize, this should happen only when hot reloading in def mode, but who knows
            definitions = [];
            _definedData.TryAdd(typ, definitions);
        }
        return definitions;
    }

    public static T[] GetObjectsOfType<T>() where T : GameDataObject
    {
        Type typ = typeof(T);
        return (T[])GetObjectsOfType(typ);
    }

    public static T? GetObject<T>(string? name) where T : GameDataObject
    {
        Type typ = typeof(T);
        return (T?)GetObject(typ, name);
    }

    public static GameDataObject? GetObject(Type typ, string? name)
    {
        Assert(Initialized);
        if (!Initialized) return null;
        if (name == null) return null;

        // todo: hash set
        GameDataObject[] objects = GetObjectsOfType(typ);
        for (int i = 0; i < objects.Length; i++)
        {
            var obj = objects[i];
            if (string.Equals(obj.Id, name, StringComparison.OrdinalIgnoreCase))
                return obj;
        }

        return null;
    }

    public static Type[]? GetDataTypes()
    {
        Assert(Initialized);
        return _definedData.Keys.ToArray();
    }

    #endregion
}