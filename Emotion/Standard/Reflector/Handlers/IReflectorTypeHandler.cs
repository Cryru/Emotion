#nullable enable

namespace Emotion.Standard.Reflector.Handlers;

public interface IReflectorTypeHandler
{
    public Type Type { get; }

    /// <summary>
    /// Whether the type handler's value can be get/set as a string.
    /// </summary>
    public bool CanGetOrParseValueAsString { get; }

    public string GetValueAsString(object instance)
    {
        throw new Exception("Not supported!");
    }

    public object? ParseValueFromString(string val)
    {
        throw new Exception("Not supported!");
    }

    public ComplexTypeHandlerMember[]? GetMembers()
    {
        return null;
    }
}
