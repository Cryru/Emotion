#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System.Runtime.InteropServices;

namespace Emotion.Standard.Reflector.Handlers;

public sealed class BooleanTypeHandler : ReflectorTypeHandlerBase<bool>
{
    public override Type Type => typeof(bool);

    public override bool CanGetOrParseValueAsString => true;

    public override TypeEditor? GetEditor()
    {
        return new BooleanEditor();
    }

    public override bool WriteValueAsString(ref ValueStringWriter stringWriter, bool instance)
    {
        return stringWriter.WriteString(instance ? bool.TrueString : bool.FalseString);
    }

    public object? ParseValueFromString(string val)
    {
        return bool.TryParse(val, out bool result);
    }

    public override bool ParseValueAsString<TReader>(TReader reader, out bool result)
    {
        ReadOnlySpan<byte> data = reader.GetDataFromCurrentPosition();
        ReadOnlySpan<char> dataAsChar = MemoryMarshal.Cast<byte, char>(data);
        return bool.TryParse(dataAsChar, out result);
    }
}
