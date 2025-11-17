#nullable enable

using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Standard.DataStructures.OptimizedStringReadWrite;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Serialization.XML;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
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

    public override string? ParseFromXML(ref ValueStringReader reader)
    {
        StringBuilder str = new StringBuilder();
        while (true)
        {
            char c = reader.ReadNextChar();
            if (c == '\0' || c == '<') break;
            str.Append(c);
        }
        return XMLRestoreString(str.ToString());
    }

    #endregion

    #region Serialization Write

    public override void WriteAsCode(string value, ref ValueStringWriter writer)
    {
        writer.WriteString($"\"{value}\"");
    }

    public override void WriteAsXML(string value, ref ValueStringWriter writer, bool addTypeTags, XMLConfig config)
    {
        if (addTypeTags)
            writer.WriteXMLTag(Type.Name, ValueStringWriter.XMLTagType.Normal);

        writer.WriteString(XMLSanitizeString(value));

        if (addTypeTags)
            writer.WriteXMLTag(Type.Name, ValueStringWriter.XMLTagType.Closing);
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
