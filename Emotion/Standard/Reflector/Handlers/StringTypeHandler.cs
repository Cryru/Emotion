#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

namespace Emotion.Standard.Reflector.Handlers;

public sealed class StringTypeHandler : ReflectorTypeHandlerBase<string>
{
    public override string TypeName => typeof(string).Name;

    public override Type Type => typeof(string);

    public override bool CanGetOrParseValueAsString => true;

    public override TypeEditor? GetEditor()
    {
        return new StringEditor();
    }

    public override bool WriteValueAsString(ref ValueStringWriter stringWriter, string? instance)
    {
        if (instance == null) return false;
        return stringWriter.WriteString(instance);
    }

    public object? ParseValueFromString(string val)
    {
        return val;
    }

    public override bool ParseValueAsString(ReadOnlySpan<char> data, out string result)
    {
        result = new string(data);
        return true;
    }
}
