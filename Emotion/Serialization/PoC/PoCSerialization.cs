#nullable enable

using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using System.Text;

namespace Emotion.Serialization.PoC;

public static class PoCSerialization
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

        if (typeHandler is IGenericReflectorComplexTypeHandler complexTypeHandler)
        {
            ComplexTypeHandlerMember[] members = complexTypeHandler.GetMembers();
            foreach (ComplexTypeHandlerMember member in members)
            {
                builder.Append(member.Name);
                builder.Append("\n");

                if (member.WriteValueAsStringFromComplexObject(builder, obj))
                {

                }
                else
                {

                }

                builder.Append("\n");
            }
        }

        return builder.ToString();
    }
}
