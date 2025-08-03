#nullable enable

#region Using

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Emotion.Core.Systems.Logging;

#endregion

namespace Emotion.Standard.Parsers.XML.TypeHandlers;

public class XMLEnumTypeHandler : XMLPrimitiveTypeHandler
{
    private DontSerializeFlagValueAttribute? _dontSerializeFlag;

    public XMLEnumTypeHandler(Type type, bool opaque) : base(type, opaque)
    {
        var isFlagEnum = type.GetCustomAttribute<FlagsAttribute>();
        if (isFlagEnum != null && Type.GetEnumUnderlyingType() == typeof(uint))
            _dontSerializeFlag = type.GetCustomAttribute<DontSerializeFlagValueAttribute>();
    }

    public override void SerializeValue(object obj, StringBuilder output, int indentation = 1, XMLRecursionChecker? recursionChecker = null)
    {
        obj = StripDontSerializeValues(obj)!;
        base.SerializeValue(obj, output, indentation, recursionChecker);
    }

    public override object? Deserialize(XMLReader input)
    {
        string readValue = input.GoToNextTag();
        if (readValue == "") return _defaultValue;

        if (Enum.TryParse(Type, readValue, out object? parsed)) return parsed;

        Engine.Log.Warning($"Couldn't find value {readValue} in enum {Type}.", MessageSource.XML, true);
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? StripDontSerializeValues(object? obj)
    {
        if (_dontSerializeFlag == null || obj == null) return obj;
        return Enum.ToObject(Type, _dontSerializeFlag.ClearDontSerialize((uint) obj));
    }
}