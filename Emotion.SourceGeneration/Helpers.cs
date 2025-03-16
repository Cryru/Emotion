using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SourceGenerator;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

        public static bool IsDerivedFrom(INamedTypeSymbol symbol, string baseTypeFullName)
        {
            while (symbol != null)
            {
                if (symbol.ToDisplayString() == baseTypeFullName)
                    return true;
                symbol = symbol.BaseType;
            }
            return false;
        }

        public static bool CanBeInitialized(INamedTypeSymbol typ)
        {
            if (typ.IsAbstract) return false;

            bool isPartial = IsPartial(typ);

            ImmutableArray<IMethodSymbol> constructors = typ.InstanceConstructors;
            if (constructors.Length == 0) return false; // huh

            foreach (IMethodSymbol constructor in constructors)
            {
                if (constructor.Parameters.IsEmpty)
                {
                    if (constructor.DeclaredAccessibility == Accessibility.Public)
                    {
                        return true;
                    }
                    else if (isPartial)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsReflectorBuiltInType(INamedTypeSymbol typ)
        {
            bool isFromEmotion = typ.ContainingAssembly.Name == "Emotion";
            bool compilingEmotion = Generator.CurrentCompilation.AssemblyName == "Emotion";
            if (isFromEmotion && !compilingEmotion) return true;

            string typName = typ.ToDisplayString();
            if (typName[typName.Length - 1] == '?') typName = typName.Substring(0, typName.Length - 1);

            if (typName == "System.ValueType") return true; // yikes
            if (typName == "System.Enum") return true; // yikes
            if (typName == "object") return true; // yikes

            if (typName == "byte") return true;
            if (typName == "ushort") return true;
            if (typName == "uint") return true;
            if (typName == "ulong") return true;

            if (typName == "sbyte") return true;
            if (typName == "short") return true;
            if (typName == "int") return true;
            if (typName == "long") return true;
            if (typName == "char") return true;

            if (typName == "float") return true;
            if (typName == "double") return true;

            if (typName == "decimal") return true;

            if (typName == "string") return true;

            if (typName == "bool") return true;

            return false;
        }

        public static bool IsReflectorableType(INamedTypeSymbol typ)
        {
            if (!HasParameterlessConstructor(typ)) return false;
            if (HasDontSerialize(typ)) return false;
            //if (typ.IsAbstract) return false;
            if (typ.DeclaredAccessibility != Accessibility.Public) return false;
            if (typ.IsGenericType && SymbolEqualityComparer.Default.Equals(typ, typ.ConstructedFrom)) return false;
            if (typ.IsRefLikeType && typ.IsValueType) return false;

            ImmutableArray<AttributeData> classAttributes = typ.GetAttributes();
            if (HasAttribute(classAttributes, "ObsoleteAttribute")) return false;

            if (IsDerivedFrom(typ, "System.IO.Stream")) return false;
            if (IsDerivedFrom(typ, "System.Attribute")) return false;

            return true;
        }

        private static string[] _excludedNamespacesSubSpaces = new string[]
        {
            "OpenGL",
            "Khronos",
            "WinApi",
            "Standart.Hash",

            "Emotion.Platform",
            "Emotion.Common",
            "Emotion.Audio",
            "Emotion.Standard.Reflector",
            "Emotion.Editor",

            "System",
        };

        public static bool IsExcludedNamespace(string namespac)
        {
            for (int i = 0; i < _excludedNamespacesSubSpaces.Length; i++)
            {
                string namespaceExclusion = _excludedNamespacesSubSpaces[i];
                if (namespac.StartsWith(namespaceExclusion))
                    return true;
            }

            return false;
        }

        public static string GetSafeName(string name)
        {
            return name.Replace(".", "").Replace("<", "Of").Replace(", ", "And").Replace(">", "").Replace("?", "").Replace("(", "").Replace(")", "");
        }
    }
}
