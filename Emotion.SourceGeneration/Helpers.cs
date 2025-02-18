using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            ImmutableArray<IMethodSymbol> constructors = typ.InstanceConstructors;
            if (constructors.Length == 0) return false;

            foreach (IMethodSymbol constructor in constructors)
            {
                if (constructor.Parameters.IsEmpty)
                    return true;
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

        public static bool HasAttribute(ImmutableArray<AttributeData> attributes, string attributeClassName)
        {
            foreach (AttributeData attribute in attributes)
            {
                string name = attribute.AttributeClass.Name.ToString();
                if (name == attributeClassName)
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

        public static bool IsPartial(INamedTypeSymbol symbol)
        {
            // generally if there is more than one - it would be partial, right?
            foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
            {
                SyntaxNode syntaxNode = syntaxReference.GetSyntax();
                if (syntaxNode is ClassDeclarationSyntax classDeclaration)
                {
                    foreach (SyntaxToken modifier in classDeclaration.Modifiers)
                    {
                        if (modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword))
                            return true;
                    }
                }
            }

            return false;
        }
    }
}
