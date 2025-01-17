using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using static Emotion.SourceGeneration.Helpers;

namespace SourceGenerator
{
    [Generator]
    public class Generator : IIncrementalGenerator
    {
        public static int generatedTypesCount = 0;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValueProvider<ImmutableArray<INamedTypeSymbol>> definedTypesProvider = context.CompilationProvider.Select(
                (compilation, cancellationToken) => GetTypesDefinedInCompilationAssembly(compilation)
            );

            context.RegisterSourceOutput(definedTypesProvider, (sourceProductionContext, definedTypes) =>
            {
                // [1st Pass] Find associated types
                HashSet<ITypeSymbol> typesToReflector = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
                var typeToMembers = new Dictionary<INamedTypeSymbol, ImmutableArray<ReflectorMemberData>>(SymbolEqualityComparer.Default);
                foreach (INamedTypeSymbol type in definedTypes)
                {
                    if (!IsReflectorableType(type)) continue;

                    Console.WriteLine($"[ReflectorV2] Generating handler for {type.ToDisplayString()}");

                    ImmutableArray<ReflectorMemberData> members = GetReflectorableTypeMembers(sourceProductionContext, type);
                    typeToMembers.Add(type, members); // Cache this for later.

                    foreach (ReflectorMemberData member in members)
                    {
                        ITypeSymbol memberType = member.TypeSymbol;
                        if (memberType.NullableAnnotation == NullableAnnotation.Annotated)
                        {
                            memberType = memberType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
                        }

                        var name = memberType.ToDisplayString();
                        if (name[name.Length - 1] == '?') continue;

                        INamedTypeSymbol namedTypeSymbol = memberType as INamedTypeSymbol;
                        if (namedTypeSymbol != null && IsReflectorBuiltInType(namedTypeSymbol)) continue;

                        if (typesToReflector.Add(memberType))
                        {
                            Console.WriteLine($"[ReflectorV2]     Associated type added {memberType.ToDisplayString()}");
                        }
                    }

                    typesToReflector.Add(type);
                }

                // [2nd pass] Generate handlers
                foreach (ITypeSymbol type in typesToReflector)
                {
                    INamedTypeSymbol typeNamed = type as INamedTypeSymbol;
                    if (typeNamed == null) continue;

                    if (!typeToMembers.TryGetValue(typeNamed, out ImmutableArray<ReflectorMemberData> members))
                        members = GetReflectorableTypeMembers(sourceProductionContext, typeNamed);

                    Console.WriteLine($"[ReflectorV2] Generating handler for {type.ToDisplayString()}.");
                    GenerateReflectorTypeHandlerForType(ref sourceProductionContext, typeNamed, members);
                }

                Console.WriteLine($"[ReflectorV2] Done!");

                // todo: list
                // todo: array
                // todo: dictionary
                // todo: generic specializations
            });
        }

        private static HashSet<string> _excludedNamespacesSubSpaces = new HashSet<string>()
        {
            "OpenGL",
            "Khronos.KhronosApi",
            "WinApi",

            "Emotion.Platform",
            "Emotion.Standard",
            "Emotion.Common",
            "Emotion.Audio",
            "Emotion.Editor",

            "System",
        };
        
        public static ImmutableArray<INamedTypeSymbol> GetTypesDefinedInCompilationAssembly(Compilation compilation)
        {
            ImmutableArray<INamedTypeSymbol>.Builder result = ImmutableArray.CreateBuilder<INamedTypeSymbol>();

            Queue<INamespaceSymbol> namespaceTraverse = new Queue<INamespaceSymbol>();
            namespaceTraverse.Enqueue(compilation.Assembly.GlobalNamespace);

            while (namespaceTraverse.Count > 0)
            {
                INamespaceSymbol namespac = namespaceTraverse.Dequeue();
                foreach (INamespaceOrTypeSymbol member in namespac.GetMembers())
                {
                    if (member is INamespaceSymbol childNamespace)
                    {
                        string displayStr = childNamespace.ToDisplayString();
                        if (_excludedNamespacesSubSpaces.Contains(displayStr))
                            continue;

                        namespaceTraverse.Enqueue(childNamespace);
                    }
                    else if (member is INamedTypeSymbol typeSymbol &&
                             typeSymbol.ContainingAssembly.Equals(compilation.Assembly, SymbolEqualityComparer.Default))
                    {
                        result.Add(typeSymbol);
                    }
                }
            }

            return result.ToImmutable();
        }

        public static bool IsReflectorBuiltInType(INamedTypeSymbol typ)
        {
            string typName = typ.ToDisplayString();
            if (typName[typName.Length - 1] == '?') typName = typName.Substring(0, typName.Length - 1);

            if (typName == "object") return true; // yikes

            if (typName == "byte") return true;
            if (typName == "ushort") return true;
            if (typName == "uint") return true;
            if (typName == "ulong") return true;

            if (typName == "sbyte") return true;
            if (typName == "short") return true;
            if (typName == "int") return true;
            if (typName == "long") return true;

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
            if (IsObsolete(classAttributes)) return false;

            return true;
        }

        public struct ReflectorMemberData
        {
            public ISymbol MemberSymbol;
            public ITypeSymbol TypeSymbol;
            public string TypeNameFull;
        }

        // Define a DiagnosticDescriptor for the warning
        private static DiagnosticDescriptor _reflectorMemberWarning = new DiagnosticDescriptor(
            id: "EMOTION001",
            title: "Reflector Member Validity Warning",
            messageFormat: "Member not marked as 'DontSerialize' but is part of serializable class",
            category: "Reflector",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static ImmutableArray<ReflectorMemberData> GetReflectorableTypeMembers(SourceProductionContext context, INamedTypeSymbol typ)
        {
            ImmutableArray<ReflectorMemberData>.Builder result = ImmutableArray.CreateBuilder<ReflectorMemberData>();

            ImmutableArray<ISymbol> members = typ.GetMembers();
            foreach (ISymbol member in members)
            {
                string memberName = member.Name;

                // Skip array indexers.
                if (memberName == "this[]") continue;

                ImmutableArray<AttributeData> memberAttributes = member.GetAttributes();

                // Skip obsolete
                if (IsObsolete(memberAttributes)) continue;
                if (HasDontSerialize(memberAttributes)) continue;

                // Skip non public.
                if (member.Kind != SymbolKind.Field && member.Kind != SymbolKind.Property) continue;
                if (member.DeclaredAccessibility != Accessibility.Public) continue;
                if (member.IsStatic) continue;

                ITypeSymbol memberType = null;
                if (member.Kind == SymbolKind.Property && member is IPropertySymbol propSymb)
                {
                    if (propSymb.GetMethod == null || propSymb.SetMethod == null) continue;
                    if (propSymb.GetMethod.DeclaredAccessibility != Accessibility.Public) continue;
                    if (propSymb.SetMethod.DeclaredAccessibility != Accessibility.Public) continue;
                    if (propSymb.SetMethod.IsInitOnly) continue;
                    memberType = propSymb.Type;
                }
                else if (member is IFieldSymbol fieldSymb)
                {
                    if (fieldSymb.IsReadOnly) continue;
                    memberType = fieldSymb.Type;
                }

                if (memberType == null) continue;

                string memberFullTypeName = memberType.ToDisplayString();

                // Pointers not supported
                if (memberFullTypeName.Contains("*")) continue;

                // Check if the type can be serialized
                INamedTypeSymbol namedMemberType = memberType as INamedTypeSymbol;
                if (namedMemberType != null)
                {
                    // Skip type can't be serialized.
                    if (HasDontSerialize(namedMemberType)) continue;

                    if (!IsReflectorBuiltInType(namedMemberType) && !IsReflectorableType(namedMemberType))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(_reflectorMemberWarning, member.Locations.FirstOrDefault()));

                        Console.WriteLine($"Error: Non reflectorable type member in reflectorable class - {memberType.ToDisplayString()} as member {member.ToDisplayString()} of {typ.ToDisplayString()}");
                        continue;
                    }
                }

                // Can't use nullable reference types in a typeof
                if (memberType.IsReferenceType && memberFullTypeName[memberFullTypeName.Length - 1] == '?')
                    memberFullTypeName = memberFullTypeName.Substring(0, memberFullTypeName.Length - 1);

                result.Add(new ReflectorMemberData()
                {
                    MemberSymbol = member,
                    TypeNameFull = memberFullTypeName,
                    TypeSymbol = memberType
                });
            }

            return result.ToImmutable();
        }

        public static void GenerateReflectorTypeHandlerForType(ref SourceProductionContext context, INamedTypeSymbol typ, ImmutableArray<ReflectorMemberData> members)
        {
            string fullTypName = typ.ToDisplayString();
            string safeName = fullTypName.Replace(".", "").Replace("<", "Of").Replace(", ", "And").Replace(">", "").Replace("?", "");

            StringBuilder sb = new StringBuilder(1024);
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("// Generated by Emotion.SourceGeneration");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using Emotion.Standard.Reflector;");
            sb.AppendLine("using Emotion.Standard.Reflector.Handlers;");
            sb.AppendLine();
            sb.AppendLine($"namespace ReflectorGen;");
            sb.AppendLine($"public static class ReflectorData{safeName}");
            sb.AppendLine("{");
            sb.AppendLine("");
            sb.AppendLine("    [ModuleInitializer]");
            sb.AppendLine("    public static void Load()");
            sb.AppendLine("    {");
            sb.AppendLine("        ReflectorEngineInit.OnInit += AttachToEngine;");
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine("    public static void AttachToEngine()");
            sb.AppendLine("    {");

            sb.AppendLine($"       ReflectorEngine.RegisterTypeHandler(new ComplexTypeHandler<{fullTypName}>(new ComplexTypeHandlerMember[]");
            sb.AppendLine($"       {{");

            foreach (var memberDesc in members)
            {
                string memberFullTypeName = memberDesc.TypeNameFull;
                string memberName = memberDesc.MemberSymbol.Name;

                sb.AppendLine($"           new ComplexTypeHandlerMember<{fullTypName}, {memberFullTypeName}>(\"{memberName}\", (p, v) => p.{memberName} = v, (p) => p.{memberName})");
                sb.AppendLine("           {");

                // Generate attributes
                ImmutableArray<AttributeData> memberAttributes = memberDesc.MemberSymbol.GetAttributes();
                if (memberAttributes.Length > 0)
                {
                    sb.AppendLine("               Attributes = new Attribute[] {");
                    foreach (var attribute in memberAttributes)
                    {
                        var clazz = attribute.AttributeClass;
                        if (clazz.Name != "VertexAttributeAttribute") continue; // todo

                        sb.AppendLine($"                   {GenerateAttributeDeclaration(attribute)},");
                    }
                    sb.AppendLine("               },");
                }

                sb.AppendLine("           },");
            }
            sb.AppendLine("       }));");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource($"RFLC.{safeName}.g.cs", sb.ToString());
        }
    }
}