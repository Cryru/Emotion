#nullable enable

using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Standard.DataStructures.OptimizedStringReadWrite;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Serialization.XML;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Emotion.Standard.Reflector.Handlers;

public sealed class StringTypeHandler : ReflectorTypeHandlerBase<string>
{
    public override string TypeName => typeof(string).Name;

    public override Type Type => typeof(string);

    #region Serialization Read

    public override string? ParseFromJSON(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            if (!reader.Read())
                return default;
        }

        Assert(reader.TokenType == JsonTokenType.String);

        return reader.GetString();
    }

    #endregion

    #region Serialization Write

    public override void WriteAsCode(string value, ref ValueStringWriter writer)
    {
        writer.WriteString($"\"{value}\"");
    }

    public override void WriteAsXML(string value, ref ValueStringWriter writer, bool addTypeTags, XMLConfig config, int indent = 0)
    {
        if (addTypeTags)
        {
            if (!writer.WriteChar('<')) return;
            if (!writer.WriteString(Type.Name)) return;
            if (!writer.WriteChar('>')) return;
        }

        writer.WriteString(XMLSanitizeString(value));

        if (addTypeTags)
        {
            if (!writer.WriteChar('<')) return;
            if (!writer.WriteString(Type.Name)) return;
            if (!writer.WriteChar('>')) return;
        }
    }

    private static readonly Regex StringSanitizeRegex = new Regex("<", RegexOptions.Compiled);
    private static readonly Regex StringRestoreRegex = new Regex("&lt;", RegexOptions.Compiled);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string XMLSanitizeString(string str)
    {
        return StringSanitizeRegex.Replace(str, "&lt;");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string XMLRestoreString(string str)
    {
        return StringRestoreRegex.Replace(str, "<");
    }

    #endregion

    public override TypeEditor? GetEditor()
    {
        return new StringEditor();
    }
}
