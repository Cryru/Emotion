#nullable enable

using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using System.Text.Json;

namespace Emotion.Standard.Reflector.Handlers;

public class DictionaryTypeHandler<T, TKey, TItem> : ReflectorTypeHandlerBase<T>, IGenericEnumerableTypeHandler
    where TKey : notnull
    where T : Dictionary<TKey, TItem>
{
    public override string TypeName => $"ArrayOf{ItemType.Name}";

    public override Type Type => typeof(T);

    public override bool CanGetOrParseValueAsString => false;

    public string KeyTypeName => KeyType.Name;

    public Type KeyType => typeof(TKey);

    public string ItemTypeName => ItemType.Name;

    public Type ItemType => typeof(TItem);

    public object CreateNew()
    {
        return new Dictionary<TKey, TItem?>();
    }

    #region Serialization Read

    public override T? ParseFromJSON(ref Utf8JsonReader reader)
    {
        // Assume this is a dictionary of string + something
        if (KeyTypeName != "String")
        {
            if (!reader.TrySkip())
                Assert(false, $"Failed skip in JSON parsing an object {TypeName}");
            return default;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            if (!reader.Read())
                return default;
        }

        Assert(reader.TokenType == JsonTokenType.StartObject);

        ReflectorTypeHandlerBase<TKey>? keyHandler = ReflectorEngine.GetTypeHandler<TKey>();
        ReflectorTypeHandlerBase<TItem>? itemHandler = ReflectorEngine.GetTypeHandler<TItem>();
        if (itemHandler == null || keyHandler == null)
        {
            if (!reader.TrySkip())
                Assert(false, $"Failed skip in JSON parsing an object {TypeName}");

            return default;
        }

        var newDict = new Dictionary<TKey, TItem>();
        string? lastKey = string.Empty;
        while (reader.Read())
        {
            JsonTokenType token = reader.TokenType;

            if (token == JsonTokenType.PropertyName)
            {
                string? key = reader.GetString();
                TItem? parsed = itemHandler.ParseFromJSON(ref reader);
                if (key != null)
                    newDict.TryAdd((TKey)(object)key, parsed!);
            }
            else if (token == JsonTokenType.EndObject)
            {
                break;
            }
            else
            {
                Assert(false, $"Unknown token {token} in JSON parsing an object {TypeName}");
            }
        }

        return (T)newDict;
    }

    #endregion

    #region Serialization Write

    // todo

    #endregion
}
