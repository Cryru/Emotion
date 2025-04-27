#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System;
using System.Buffers;
using System.Globalization;
using System.Text.Json;

namespace Emotion.Standard.Reflector.Handlers;

public sealed class PrimitiveNumericTypeHandler<T> : ReflectorTypeHandlerBase<T> where T : unmanaged, INumber<T>
{
    public override string TypeName => typeof(T).Name;

    public override Type Type => typeof(T);

    public override bool CanGetOrParseValueAsString => true;

    private TypeCode _typeCode;

    public PrimitiveNumericTypeHandler()
    {
        _typeCode = Type.GetTypeCode(Type);
    }

    #region Serialization Read

    public override T ParseFromJSON(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            if (!reader.Read())
                return default;
        }

        Assert(reader.TokenType == JsonTokenType.Number);

        switch (_typeCode)
        {
            case TypeCode.Single:
                {
                    reader.TryGetSingle(out float v);
                    return T.CreateSaturating(v);
                }
            case TypeCode.Double:
                {
                    reader.TryGetDouble(out double v);
                    return T.CreateSaturating(v);
                }

            case TypeCode.Byte:
                {
                    reader.TryGetByte(out byte v);
                    return T.CreateSaturating(v);
                }
            case TypeCode.SByte:
                {
                    reader.TryGetSByte(out sbyte v);
                    return T.CreateSaturating(v);
                }

            case TypeCode.Int16:
                {
                    reader.TryGetInt16(out short v);
                    return T.CreateSaturating(v);
                }
            case TypeCode.UInt16:
                {
                    reader.TryGetUInt16(out ushort v);
                    return T.CreateSaturating(v);
                }

            case TypeCode.Int32:
                {
                    reader.TryGetInt32(out int v);
                    return T.CreateSaturating(v);
                }
            case TypeCode.UInt32:
                {
                    reader.TryGetUInt32(out uint v);
                    return T.CreateSaturating(v);
                }

            case TypeCode.Int64:
                {
                    reader.TryGetInt64(out long v);
                    return T.CreateSaturating(v);
                }
            case TypeCode.UInt64:
                {
                    reader.TryGetUInt64(out ulong v);
                    return T.CreateSaturating(v);
                }
            default:
                return T.Zero;
        }
    }

    #endregion

    #region Serialization Write

    public override void WriteAsCode(T value, ref ValueStringWriter writer)
    {
        writer.WriteNumber(value);
    }

    #endregion

    public override TypeEditor? GetEditor()
    {
        return new NumberEditor<T>();
    }

    public override bool WriteValueAsString(ref ValueStringWriter stringWriter, T instance)
    {
        return stringWriter.WriteNumber(instance);
    }

    public object? ParseValueFromString(string val)
    {
        bool success = T.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out T result);
        if (success) return result;
        return T.Zero;
    }

    public override bool ParseValueAsString(ReadOnlySpan<char> data, out T result)
    {
        bool success = T.TryParse(data, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        if (!success) result = T.Zero;
        return success;
    }
}
