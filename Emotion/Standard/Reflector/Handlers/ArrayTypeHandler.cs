#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.Utility;
using System.Text.Json;

namespace Emotion.Standard.Reflector.Handlers;

public class ArrayTypeHandler<T, TItem> : ReflectorTypeHandlerBase<T>, IGenericEnumerableTypeHandler where T : IEnumerable<TItem>
{
    public override string TypeName => $"ArrayOf{ItemType.Name}";

    public override Type Type => typeof(T);

    public string ItemTypeName => ItemType.Name;

    public Type ItemType => typeof(TItem);

    public object CreateNew()
    {
        return Array.Empty<TItem>();
    }

    private static ObjectPool<List<TItem?>> _pool = new ObjectPool<List<TItem?>>((l) => l.Clear(), 1);

    #region Serialization Read

    public override T? ParseFromJSON(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            if (!reader.Read())
                return default;
        }

        Assert(reader.TokenType == JsonTokenType.StartArray);

        ReflectorTypeHandlerBase<TItem>? itemHandler = ReflectorEngine.GetTypeHandler<TItem>();
        if (itemHandler == null)
        {
            if (!reader.TrySkip())
                Assert(false, $"Failed skip in JSON parsing an object {TypeName}");

            return default;
        }

        List<TItem?> tempList = _pool.Get();
        while (reader.Read())
        {
            JsonTokenType token = reader.TokenType;
            if (token == JsonTokenType.StartObject || token == JsonTokenType.StartArray)
            {
                TItem? item = itemHandler.ParseFromJSON(ref reader);
                tempList.Add(item);
            }
            else if (token == JsonTokenType.Number || token == JsonTokenType.String ||
                token == JsonTokenType.True || token == JsonTokenType.False)
            {
                TItem? item = itemHandler.ParseFromJSON(ref reader);
                tempList.Add(item);
            }
            else if (token == JsonTokenType.Null)
            {
                tempList.Add((TItem?)(object?)null);
            }
            else if (token == JsonTokenType.EndArray)
            {
                break;
            }
            else
            {
                Assert(false, $"Unknown token {token} in JSON parsing an object {TypeName}");
            }
        }

        TItem[] values = new TItem[tempList.Count];
        tempList.CopyTo(values, 0);

        _pool.Return(tempList);

        return (T?)(object?)values;
    }

    #endregion

    #region Serialization Write

    public override void WriteAsCode(T? value, ref ValueStringWriter writer)
    {
        if (value == null)
        {
            writer.WriteString("null");
            return;
        }

        ReflectorTypeHandlerBase<TItem>? itemHandler = ReflectorEngine.GetTypeHandler<TItem>();
        if (itemHandler == null)
            return;

        bool first = true;
        writer.WriteString("[\n");
        foreach (var item in value)
        {
            if (!first) writer.WriteString(",\n");
            itemHandler.WriteAsCode(item, ref writer);

            first = false;
        }
        writer.WriteString("\n]");
    }

    #endregion
}
