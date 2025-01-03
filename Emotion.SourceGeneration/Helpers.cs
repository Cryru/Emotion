using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Emotion.SourceGeneration
{
    public static class Helpers
    {
        public static bool HasParameterlessConstructor(INamedTypeSymbol typ)
        {
            var constructors = typ.InstanceConstructors;
            if (constructors.Length == 0) return false;

            foreach (var constructor in constructors)
            {
                if (constructor.Parameters.IsEmpty)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasDontSerialize(INamedTypeSymbol typ)
        {
            INamedTypeSymbol typeToCheck = typ;
            while (typeToCheck != null)
            {
                var attributes = typeToCheck.GetAttributes();
                foreach (var attribute in attributes)
                {
                    if (attribute.AttributeClass.Name == "DontSerializeAttribute")
                    {
                        return true;
                    }
                }

                typeToCheck = typeToCheck.BaseType;
            }

            return false;
        }

        public static bool HasDontSerialize(ImmutableArray<AttributeData> attributes)
        {
            foreach (var attribute in attributes)
            {
                string name = attribute.AttributeClass.Name.ToString();
                if (name == "DontSerializeAttribute")
                    return true;
            }
            return false;
        }

        public static string GenerateAttributeDeclaration(AttributeData attribute)
        {
            var constructorArgs = string.Join(", ", attribute.ConstructorArguments.Select(arg => GetArgumentString(arg)).Where(x => x != null));
            var namedArgs = string.Join(", ", attribute.NamedArguments.Select(kvp => $"{kvp.Key} = {GetArgumentString(kvp.Value)}"));

            var args = constructorArgs;
            if (!string.IsNullOrEmpty(namedArgs))
            {
                if (!string.IsNullOrEmpty(constructorArgs))
                {
                    args += ", ";
                }
                args += namedArgs;
            }

            string fullAttributeName = attribute.AttributeClass?.ToDisplayString();
            return $"new {fullAttributeName}({args})";
        }

        public static string GetArgumentString(TypedConstant argument)
        {
            if (argument.Kind == TypedConstantKind.Array)
            {
                var elements = argument.Values.Select(GetArgumentString);
                return $"new {argument.Type.ToDisplayString()} {{ {string.Join(", ", elements)} }}";
            }
            else if (argument.Kind == TypedConstantKind.Enum)
            {
                return $"{argument.Type.ToDisplayString()}.{argument.Value}";
            }
            else if (argument.Kind == TypedConstantKind.Type)
            {
                if (argument.Value == null) return null;
                return $"typeof({argument.Value})";
            }
            else if (argument.Value is string str)
            {
                return $"\"{str}\"";
            }
            else if (argument.Value is char c)
            {
                return $"'{c}'";
            }
            else if (argument.Value is null)
            {
                return "null";
            }
            else if (argument.Value is bool)
            {
                return argument.Value.ToString().ToLowerInvariant();
            }
            else
            {
                return argument.Value?.ToString() ?? string.Empty;
            }
        }

        public static bool IsObsolete(ImmutableArray<AttributeData> attributes)
        {
            foreach (var attribute in attributes)
            {
                string name = attribute.AttributeClass.Name.ToString();
                if (name == "ObsoleteAttribute")
                    return true;
            }
            return false;
        }
    }
}
