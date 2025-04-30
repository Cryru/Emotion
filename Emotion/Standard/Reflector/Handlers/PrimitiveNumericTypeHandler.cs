#nullable enable

using Emotion.Serialization.XML;
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

    private TypeCode _typeCode;
    private NumberStyles _numberStyle;

    public PrimitiveNumericTypeHandler()
    {
        _typeCode = Type.GetTypeCode(Type);
        _numberStyle = _typeCode == TypeCode.Single || _typeCode == TypeCode.Double ? NumberStyles.Float : NumberStyles.Integer;
    }

    public override TypeEditor? GetEditor()
    {
        return new NumberEditor<T>();
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

    public override T ParseFromXML(ref ValueStringReader reader)
    {
        char c = reader.ReadNextChar();
        if (c != '>') return default;
        
        Span<char> readMemory = stackalloc char[128];
        int readChars = 0;
        
        while (true)
        {
            var nextChar = reader.ReadNextChar();
            if (nextChar == '\0' || nextChar == '<') break;

            readMemory[readChars] = nextChar;
            readChars++;

            if (readChars == readMemory.Length) break;
        }

        if (T.TryParse(readMemory.Slice(0, readChars), _numberStyle, CultureInfo.InvariantCulture, out T result))
            return result;

        return T.Zero;
    }

    #endregion

    #region Serialization Write

    public override void WriteAsCode(T value, ref ValueStringWriter writer)
    {
        writer.WriteNumber(value);
    }

    public override void WriteAsXML(T value, ref ValueStringWriter writer, bool addTypeTags, XMLConfig config, int indent = 0)
    {
        if (addTypeTags)
        {
            if (!writer.WriteChar('<')) return;
            if (!writer.WriteString(Type.Name)) return;
            if (!writer.WriteChar('>')) return;
        }

        writer.WriteNumber(value);

        if (addTypeTags)
        {
            if (!writer.WriteString("</")) return;
            if (!writer.WriteString(Type.Name)) return;
            if (!writer.WriteChar('>')) return;
        }
    }

    #endregion
}
