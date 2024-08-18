#nullable enable

namespace Emotion.Standard.Reflector;

public class ReflectorTypeMember
{
    private static Action<dynamic, dynamic?> _emptyWriteFunc = (a, b) => { };
    private static Func<dynamic, dynamic?> _emptyReadFunc = (a) => { return null; };

    public Type MemberType;
    public string Name;
    public Action<dynamic, dynamic?> WriteValue = _emptyWriteFunc;
    public Func<dynamic, dynamic?> ReadValue = _emptyReadFunc;
    public Attribute[] Attributes = Array.Empty<Attribute>();

    public ReflectorTypeMember(Type type, string name)
    {
        MemberType = type;
        Name = name;
    }

    public T? HasAttribute<T>() where T : Attribute
    {
        for (int i = 0; i < Attributes.Length; i++)
        {
            var attribute = Attributes[i];
            if (attribute is T attributeAsT) return attributeAsT;
        }
        return null;
    }
}
