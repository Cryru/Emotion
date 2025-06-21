using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System.Text.Json;

namespace Emotion.Standard.Reflector.Handlers;

#nullable enable

public class EnumTypeHandler<T, TNum> : ReflectorTypeHandlerBase<T>, IReflectorEnumHandler
    where T : Enum
    where TNum : INumber<TNum>
{
    public override string TypeName => typeof(T).Name;

    public override Type Type => typeof(T);

    public Type UnderlyingType => typeof(TNum);

    private Dictionary<string, (T, TNum)> _items;
    private Dictionary<TNum, T> _itemsNumeric;
    private Dictionary<T, string> _itemToString;
    private string[] _names;
    private T[] _values;

    private string _fullEnumName;

    public EnumTypeHandler(Dictionary<string, (T enumVal, TNum underVal)> items)
    {
        _fullEnumName = $"global::{typeof(T).FullName}";
        _items = items;

        int curIdx = 0;
        _names = new string[_items.Count];
        _values = new T[_items.Count];
        _itemsNumeric = new();
        _itemToString = new();
        foreach (var item in items)
        {
            // It is possible to have duplicate under values with different names.
            // todo: handle string parsing of these
            _itemsNumeric.TryAdd(item.Value.underVal, item.Value.enumVal);
            _itemToString.TryAdd(item.Value.enumVal, item.Key);

            _names[curIdx] = item.Key;
            _values[curIdx] = item.Value.enumVal;
            curIdx++;
        }
    }

    public override TypeEditor GetEditor()
    {
        return new EnumEditor<T, TNum>();
    }

    #region Serialization Read

    public override T? ParseFromJSON(ref Utf8JsonReader reader)
    {
        JsonTokenType token = reader.TokenType;
        if (token != JsonTokenType.Number)
        {
            if (!reader.Read())
                return default;
        }

        ReflectorTypeHandlerBase<TNum>? underlyingHandler = ReflectorEngine.GetTypeHandler<TNum>();
        if (underlyingHandler == null)
            return default;

        TNum? val = underlyingHandler.ParseFromJSON(ref reader);
        if (val == null)
            return default;

        if (TryParse(val, out T? result))
            return result;

        return default;
    }

    #endregion

    #region Serialization Write

    public override void WriteAsCode(T value, ref ValueStringWriter writer)
    {
        writer.WriteString($"{_fullEnumName}.{GetValueName(value)}");
    }

    #endregion

    public string[] GetNames()
    {
        return _names;
    }

    public Array GetValuesGeneric()
    {
        return _values;
    }

    public T[] GetValues()
    {
        return _values;
    }

    public string GetValueName(T value)
    {
        _itemToString.TryGetValue(value, out string valName);
        valName ??= string.Empty;
        return valName;
    }

    public string GetValueName(object value)
    {
        if (value is T valAsT && _itemToString.TryGetValue(valAsT, out string valName))
            return valName;
        return string.Empty;
    }

    public bool TryParse(string str, out T? result)
    {
        result = default;

        if (_items.TryGetValue(str, out (T, TNum) val))
        {
            result = val.Item1;
            return true;
        }

        return false;
    }

    public bool TryParse(TNum numeric, out T? result)
    {
        return _itemsNumeric.TryGetValue(numeric, out result);
    }

    public bool TryParse(string str, out object? result)
    {
        result = default;

        if (_items.TryGetValue(str, out (T, TNum) val))
        {
            result = val.Item1;
            return true;
        }

        return false;
    }

    public bool TryParse<TUnderlyingNumeric>(TUnderlyingNumeric numeric, out object? result) where TUnderlyingNumeric : INumber<TUnderlyingNumeric>
    {
        result = default;

        TNum asUnderlying = TNum.CreateSaturating(numeric);
        if (_itemsNumeric.TryGetValue(asUnderlying, out T? val))
        {
            result = val;
            return true;
        }

        return false;
    }
}
