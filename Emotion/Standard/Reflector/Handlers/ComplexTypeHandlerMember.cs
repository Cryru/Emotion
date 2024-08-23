#nullable enable

namespace Emotion.Standard.Reflector.Handlers;

public abstract class ComplexTypeHandlerMember
{
    public Type Type { get; protected set; }

    public string Name { get; protected set; }

    public Attribute[] Attributes = Array.Empty<Attribute>();

    protected ComplexTypeHandlerMember(Type memberType, string name)
    {
        Type = memberType;
        Name = name;
    }

    public abstract IReflectorTypeHandler? GetTypeHandler();

    public T? HasAttribute<T>() where T : Attribute
    {
        for (int i = 0; i < Attributes.Length; i++)
        {
            var attribute = Attributes[i];
            if (attribute is T attributeAsT) return attributeAsT;
        }
        return null;
    }

    public abstract bool ReadValueFromComplexObject(object obj, out object? readValue);

    public abstract bool SetValueInComplexObject(object obj, object val);
}

public class ComplexTypeHandlerMember<ParentT, MyT> : ComplexTypeHandlerMember
{
    protected Action<ParentT, MyT> _setValue;
    protected Func<ParentT, MyT> _getValue;

    public ComplexTypeHandlerMember(string name, Action<ParentT, MyT> setValue, Func<ParentT, MyT> getValue) : base(typeof(MyT), name)
    {
        _setValue = setValue;
        _getValue = getValue;
    }

    public override IReflectorTypeHandler? GetTypeHandler()
    {
        return ReflectorEngine.GetTypeHandler(Type);
    }

    public override bool ReadValueFromComplexObject(object obj, out object? readValue)
    {
        readValue = null;
        if (obj is ParentT parentType)
        {
            readValue = _getValue(parentType);
            return true;
        }
        return false;
    }

    public override bool SetValueInComplexObject(object obj, object val)
    {
        if (obj is ParentT parentType && val is MyT valType)
        {
            _setValue(parentType, valType);
            return true;
        }
        return false;
    }
}
