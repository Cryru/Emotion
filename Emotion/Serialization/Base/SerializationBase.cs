#nullable enable

using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using System.Text;

namespace Emotion.Serialization.Base;

public static class SerializationBase
{
    public static string Serialize<T>(T obj)
    {
        if (obj == null) return "null";

        Type type = obj.GetType();
        IGenericReflectorTypeHandler? typeHandler = ReflectorEngine.GetTypeHandler(type);
        if (typeHandler == null) return "null";

        StringBuilder builder = new StringBuilder();

        if (typeHandler.CanGetOrParseValueAsString)
        {
            typeHandler.WriteValueAsStringGeneric(builder, obj);
            return builder.ToString();
        }

        builder.Append(type.Name);
        builder.Append("\n");

        if (typeHandler is ComplexTypeHandler complexTypeHandler)
        {
            ComplexTypeHandlerMember[] members = complexTypeHandler.GetMembers();
            foreach (ComplexTypeHandlerMember member in members)
            {
                builder.Append(member.Name);
                builder.Append("\n");

                if (member.WriteValueFromComplexObject(builder, obj))
                {

                }
                else
                {

                }

                //if (member.ReadValueFromComplexObject(obj, out object? readValue))
                //{
                //    var memberTypeHandler = member.GetTypeHandler();
                //    if (memberTypeHandler != null && memberTypeHandler.CanGetOrParseValueAsString)
                //    {
                //        string valAsString = readValue == null ? "null" : memberTypeHandler.GetValueAsString(readValue);
                //        builder.Append(valAsString);
                //    }
                //    else
                //    {
                //        builder.Append("??");
                //    }
                //}

                builder.Append("\n");
            }
        }

        //if (typeHandler != null && typeHandler.CanGetOrParseValueAsString)
        //{
        //    return null;
        //}

        return builder.ToString();
    }
}
