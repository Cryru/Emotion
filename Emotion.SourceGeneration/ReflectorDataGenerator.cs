using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;

namespace SourceGenerator
{
    [Generator]
    public class ReflectorDataGenerator : ISourceGenerator
    {
        private static HashSet<string> _excludedNamespacesSubSpaces = new HashSet<string>()
        {
            "Silk",
            "SixLabors",
            "Roslyn",
            "WinApi",
            "FxResources",
            "vc",
            "Microsoft",
            "Accessibility",
            "System",
            "OpenGL",
            "std",
            "<CppImplementationDetails>",
            "<CrtImplementationDetails>",
            "LinqToSqlShared",
            "Emotion.Platform",
            "Emotion.Standard",
            "Emotion.Common",
            "Emotion.Audio",
            "Emotion.Editor",
        };
        private static HashSet<string> _excludedNamespacesTypes = new HashSet<string>()
        {
            "<global namespace>" // Global namespace
        };
        private static HashSet<string> _forceIncludeSubspaces = new HashSet<string>()
        {
            "System.Numerics"
        };

        private StringBuilder _reflectorTypeGetCodeGen = new StringBuilder();
        private string _mainNamespace;

        public void GenerateReflectorForNamespace(ref GeneratorExecutionContext context, INamespaceSymbol space, bool includeTypes = true)
        {
            ImmutableArray<INamedTypeSymbol> types = space.GetTypeMembers();
            string fullSpaceName = space.ToDisplayString();

            bool includeTypesFromNamespace = includeTypes && !_excludedNamespacesTypes.Contains(fullSpaceName);
            bool includeSubSpaces = includeTypes && !_excludedNamespacesSubSpaces.Contains(fullSpaceName);
            if (!includeSubSpaces) includeTypesFromNamespace = false;

            if (_forceIncludeSubspaces.Contains(fullSpaceName))
            {
                includeTypesFromNamespace = true;
                includeSubSpaces = true;
            }

            if (includeTypesFromNamespace)
            {
                Console.WriteLine($"Generating Reflector for types in namespace - {fullSpaceName}");
                foreach (var type in types)
                {
                    GenerateReflectorForType(ref context, type);
                }
            }

            IEnumerable<INamespaceSymbol> subSpaces = space.GetNamespaceMembers();
            foreach (var subspace in subSpaces)
            {
                GenerateReflectorForNamespace(ref context, subspace, includeSubSpaces);
            }
        }

        public void GenerateReflectorForType(ref GeneratorExecutionContext context, INamedTypeSymbol typ)
        {
            if (!HasParameterlessConstructor(typ)) return;
            if (HasDontSerialize(typ)) return;
            if (typ.IsAbstract) return;
            if (typ.DeclaredAccessibility != Accessibility.Public) return;
            if (typ.IsGenericType) return;

            // Emotion generates its own reflection data.
            if (typ.ContainingAssembly.Name == "Emotion" && _mainNamespace != "Emotion") return;

            string fullTypName = typ.ToDisplayString();

            Console.WriteLine($"    Generating Reflector for type - {fullTypName}");
            _reflectorTypeGetCodeGen.AppendLine($"          ReflectorEngine.RegisterTypeHandler(new ComplexTypeHandler<{fullTypName}>(new ComplexTypeHandlerMember[]");
            _reflectorTypeGetCodeGen.AppendLine($"          {{");

            ImmutableArray<ISymbol> members = typ.GetMembers();
            foreach (ISymbol member in members)
            {
                if (member.Kind != SymbolKind.Field && member.Kind != SymbolKind.Property) continue;
                if (member.DeclaredAccessibility != Accessibility.Public) continue;
                if (member.IsStatic) continue;

                ITypeSymbol memberType = null;
                if (member.Kind == SymbolKind.Property && member is IPropertySymbol propSymb)
                {
                    if (propSymb.GetMethod == null || propSymb.SetMethod == null) continue;
                    if (propSymb.GetMethod.DeclaredAccessibility != Accessibility.Public) continue;
                    if (propSymb.SetMethod.DeclaredAccessibility != Accessibility.Public) continue;
                    memberType = propSymb.Type;
                }
                else if (member is IFieldSymbol fieldSymb)
                {
                    if (fieldSymb.IsReadOnly) continue;
                    memberType = fieldSymb.Type;
                }

                string memberName = member.Name;
                if (memberName == "this[]") continue;

                string memberFullTypeName = memberType?.ToDisplayString();

                // Pointers not supported
                if (memberFullTypeName.Contains("*")) continue;
  
                // Can't use nullable reference types in a typeof
                if (memberType != null && memberType.IsReferenceType && memberFullTypeName[memberFullTypeName.Length - 1] == '?')
                    memberFullTypeName = memberFullTypeName.Substring(0, memberFullTypeName.Length - 1);
                _reflectorTypeGetCodeGen.AppendLine($"              new ComplexTypeHandlerMember<{fullTypName}, {memberFullTypeName}>(\"{memberName}\", (p, v) => p.{memberName} = v, (p) => p.{memberName})");
                _reflectorTypeGetCodeGen.AppendLine($"              {{");

                var attributes = member.GetAttributes();
                if (attributes.Length > 0)
                {
                    _reflectorTypeGetCodeGen.AppendLine($"                  Attributes = new Attribute[] {{");
                    foreach (var attribute in attributes)
                    {
                        _reflectorTypeGetCodeGen.AppendLine($"                      {GenerateAttributeDeclaration(attribute)},");
                    }
                    _reflectorTypeGetCodeGen.AppendLine($"                  }},");
                }

                _reflectorTypeGetCodeGen.AppendLine($"              }},");
            }

            _reflectorTypeGetCodeGen.AppendLine($"          }}));");
        }

        public bool HasDontSerialize(INamedTypeSymbol typ)
        {
            INamedTypeSymbol typeToCheck = typ;
            while (typeToCheck != null && !SymbolEqualityComparer.Default.Equals(typeToCheck, typ.BaseType))
            {
                var attributes = typ.GetAttributes();
                foreach (var attribute in attributes)
                {
                    if (attribute.AttributeClass.Name == "DontSerializeAttribute")
                    {
                        return true;
                    }
                }

                typeToCheck = typ.BaseType;
            }

            return false;
        }

        public bool HasParameterlessConstructor(INamedTypeSymbol typ)
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

        private string GenerateAttributeDeclaration(AttributeData attribute)
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

        private string GetArgumentString(TypedConstant argument)
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

        public void Execute(GeneratorExecutionContext context)
        {
            var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);
            if (mainMethod == null)
                _mainNamespace = "Emotion";
            else
                _mainNamespace = mainMethod.ContainingNamespace.ToDisplayString();

            _reflectorTypeGetCodeGen.AppendLine("// <auto-generated/>");
            _reflectorTypeGetCodeGen.AppendLine("// Generated by Emotion.SourceGeneration");
            _reflectorTypeGetCodeGen.AppendLine("");
            _reflectorTypeGetCodeGen.AppendLine("using System;");
            _reflectorTypeGetCodeGen.AppendLine("using System.Collections.Generic;");
            _reflectorTypeGetCodeGen.AppendLine("using System.Runtime.CompilerServices;");
            _reflectorTypeGetCodeGen.AppendLine("using Emotion.Standard.Reflector;");
            _reflectorTypeGetCodeGen.AppendLine("using Emotion.Standard.Reflector.Handlers;");
            _reflectorTypeGetCodeGen.AppendLine("");
            _reflectorTypeGetCodeGen.AppendLine($"namespace {_mainNamespace}");
            _reflectorTypeGetCodeGen.AppendLine("{");
            _reflectorTypeGetCodeGen.AppendLine("   public static class ReflectorData");
            _reflectorTypeGetCodeGen.AppendLine("   {");
            _reflectorTypeGetCodeGen.AppendLine("");
            _reflectorTypeGetCodeGen.AppendLine("       [ModuleInitializer]");
            _reflectorTypeGetCodeGen.AppendLine("       public unsafe static void Initialize()");
            _reflectorTypeGetCodeGen.AppendLine("       {");

            INamespaceSymbol globalNamespace = context.Compilation.GlobalNamespace;
            GenerateReflectorForNamespace(ref context, globalNamespace);

            _reflectorTypeGetCodeGen.AppendLine("       }");
            _reflectorTypeGetCodeGen.AppendLine("   }");
            _reflectorTypeGetCodeGen.AppendLine("}");

            string reflectorFileContent = _reflectorTypeGetCodeGen.ToString();
            context.AddSource($"ReflectorData.generated.cs", reflectorFileContent);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // nop
        }
    }
}