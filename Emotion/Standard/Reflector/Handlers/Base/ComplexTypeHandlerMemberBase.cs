#nullable enable

using Emotion.Standard.DataStructures.OptimizedStringReadWrite;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.Standard.Serialization.XML;
using System.Text.Json;

namespace Emotion.Standard.Reflector.Handlers.Base;

public abstract class ComplexTypeHandlerMemberBase
{
    public string ParentTypeName { get; init; }

    public Type ParentType { get; init; }

    public string TypeName { get; init; }

    public Type Type { get; init; }

    public string Name { get; init; }

    public Attribute[] Attributes { get; set; } = Array.Empty<Attribute>();

    protected ComplexTypeHandlerMemberBase(Type parentType, Type childType, string memberName)
    {
        ParentTypeName = parentType.Name;
        ParentType = parentType;

        TypeName = childType.Name;
        Type = childType;

        Name = memberName;
    }

    public abstract bool IsValueDefault<TOwner>(TOwner owner);

    public abstract void PostInit();

    public T? HasAttribute<T>() where T : Attribute
    {
        for (int i = 0; i < Attributes.Length; i++)
        {
            var attribute = Attributes[i];
            if (attribute is T attributeAsT) return attributeAsT;
        }
        return null;
    }

    public abstract IGenericReflectorTypeHandler? GetTypeHandler();

    #region Member API

    public abstract bool GetValueFromComplexObject(object obj, out object? readValue);

    public abstract bool SetValueInComplexObject(object obj, object? val);

    // Needed to avoid struct boxing
    public abstract object? SetValueInComplexObjectAndReturnParent(object obj, object? val);

    #endregion

    #region Serialization Read

    public abstract bool ParseFromXML<ParentT>(ref ValueStringReader reader, ParentT intoObject);

    public abstract bool ParseFromJSON<ParentT>(ref Utf8JsonReader reader, ParentT intoObject);

    #endregion

    #region Serialization Write

    public abstract void WriteAsCode<OwnerT>(OwnerT ownerObject, ref ValueStringWriter writer);

    public abstract void WriteAsXML<OwnerT>(OwnerT ownerObject, ref ValueStringWriter writer, bool addTypeTags, XMLConfig config, int indent = 0);

    #endregion
}