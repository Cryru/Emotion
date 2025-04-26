using Emotion.SourceGeneration;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using static Emotion.SourceGeneration.Helpers;

namespace SourceGenerator
{
    [Generator]
    public class Generator : IIncrementalGenerator
    {
        public static int generatedTypesCount = 0;

        public static Compilation CurrentCompilation = null;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValueProvider<ImmutableArray<INamedTypeSymbol>> definedTypesProvider = context.CompilationProvider.Select(
                (compilation, cancellationToken) =>
                {
                    CurrentCompilation = compilation;

                    // [Step 0] Find all types defined that are not excluded
                    return GetTypesDefinedInCompilationAssembly(compilation);
                }
            );

            context.RegisterSourceOutput(definedTypesProvider, (sourceProductionContext, definedTypes) =>
            {
                // [Step 1] Find associated types of the complex types (base types, members), element types of arrays, and so forth...
                // These types would be from other assemblies. (todo: what if the other assembly has reflector as well?)
                HashSet<ITypeSymbol> typesToReflector = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
                var typeToMembers = new Dictionary<INamedTypeSymbol, ImmutableArray<ReflectorMemberData>>(SymbolEqualityComparer.Default);
                foreach (INamedTypeSymbol type in definedTypes)
                {
                    GameDataEditorSupportGenerator.Run(ref sourceProductionContext, type);
                    ReflectorStaticClassGenerator.Run(ref sourceProductionContext, type);

                    if (type.IsStatic) continue;
                    AddComplexTypeAndAssociatedTypes(sourceProductionContext, type, typesToReflector, typeToMembers);
                }

                // [Step 2] Generate handlers for all types found
                foreach (ITypeSymbol type in typesToReflector)
                {
                    if (type.TypeKind == TypeKind.Array && type is IArrayTypeSymbol arrayType)
                    {
                        Console.WriteLine($"[ReflectorV2] Generating array handler for {type.ToDisplayString()}.");
                        GenerateHandlerForArrayType(ref sourceProductionContext, arrayType);
                    }
                    else
                    {
                        INamedTypeSymbol typeNamed = type as INamedTypeSymbol;
                        if (typeNamed == null) continue;

                        // Get member list of complex type
                        if (!typeToMembers.TryGetValue(typeNamed, out ImmutableArray<ReflectorMemberData> members))
                            members = GetReflectorableTypeMembers(sourceProductionContext, typeNamed);

                        Console.WriteLine($"[ReflectorV2] Generating handler for {type.ToDisplayString()}.");
                        GenerateHandlerForComplexType(ref sourceProductionContext, typeNamed, members);
                    }
                }

                Console.WriteLine($"[ReflectorV2] Done!");

                // todo: list
                // todo: dictionary
                // todo: generic specializations
            });
        }

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
                        if (IsExcludedNamespace(displayStr)) continue;

                        namespaceTraverse.Enqueue(childNamespace);
                    }
                    // Check if the type is in the current assembly
                    else if (member is INamedTypeSymbol typeSymbol && typeSymbol.ContainingAssembly.Equals(compilation.Assembly, SymbolEqualityComparer.Default))
                    {
                        result.Add(typeSymbol);

                        // Add nested classes.
                        ImmutableArray<INamedTypeSymbol> typeMembers = typeSymbol.GetTypeMembers();
                        for (int i = 0; i < typeMembers.Length; i++)
                        {
                            INamedTypeSymbol nestedType = typeMembers[i];
                            if (nestedType.DeclaredAccessibility == Accessibility.Public)
                                result.Add(nestedType);
                        }
                    }
                }
            }

            return result.ToImmutable();
        }

        public struct ReflectorMemberData
        {
            public ISymbol MemberSymbol;
            public ITypeSymbol TypeSymbol;
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

        private static DiagnosticDescriptor _reflectorPrivateMember = new DiagnosticDescriptor(
            id: "EMOTION002",
            title: "Reflector Member Validity Warning",
            messageFormat: "Member is marked as 'SerializeNonPublicGetSetAttribute' but the class it is part of is not marked as 'partial'",
            category: "Reflector",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public static ImmutableArray<ReflectorMemberData> GetReflectorableTypeMembers(SourceProductionContext context, INamedTypeSymbol typ, bool staticMode = false)
        {
            ImmutableArray<ReflectorMemberData>.Builder result = ImmutableArray.CreateBuilder<ReflectorMemberData>();

            ImmutableArray<ISymbol> members = typ.GetMembers();
            foreach (ISymbol member in members)
            {
                string memberName = member.Name;

                // Exclude function members.
                if (member.Kind != SymbolKind.Field && member.Kind != SymbolKind.Property) continue;

                // For static complex type handlers we want only static members,
                // for everything else we want non-static members only.
                if (staticMode)
                {
                    if (!member.IsStatic) continue;
                }
                else
                {
                    if (member.IsStatic) continue;
                }
                

                // Skip indexer properties.
                if (memberName == "this[]") continue;

                ImmutableArray<AttributeData> memberAttributes = member.GetAttributes();

                // Skip obsolete types
                if (HasAttribute(memberAttributes, "ObsoleteAttribute")) continue;
                if (HasAttribute(memberAttributes, "DontSerializeAttribute")) continue;

                // Skip non-public members, unless annotated.
                bool allowNonPublic = HasAttribute(memberAttributes, "SerializeNonPublicGetSetAttribute");
                if (allowNonPublic && !IsPartial(typ))
                {
                    allowNonPublic = false;
                    context.ReportDiagnostic(Diagnostic.Create(_reflectorPrivateMember, typ.Locations.FirstOrDefault()));
                }

                if (allowNonPublic)
                {
                    IAssemblySymbol containingAssembly = typ.ContainingAssembly;
                    Compilation compilationAssembly = CurrentCompilation;
                    bool isFromCompilationAssembly = containingAssembly.Equals(compilationAssembly.Assembly, SymbolEqualityComparer.Default);
                    if (!isFromCompilationAssembly) allowNonPublic = false;
                }

                if (!allowNonPublic && member.DeclaredAccessibility != Accessibility.Public) continue;

                // If a property check if it can be get/set at all.
                ITypeSymbol memberType = null;
                if (member.Kind == SymbolKind.Property && member is IPropertySymbol propSymb)
                {
                    if (propSymb.GetMethod == null || propSymb.SetMethod == null) continue;

                    if (!allowNonPublic)
                    {
                        if (propSymb.GetMethod.DeclaredAccessibility != Accessibility.Public) continue;
                        if (propSymb.SetMethod.DeclaredAccessibility != Accessibility.Public) continue;
                    }

                    if (propSymb.SetMethod.IsInitOnly) continue;
                    memberType = propSymb.Type;
                }
                else if (member is IFieldSymbol fieldSymb)
                {
                    if (fieldSymb.IsReadOnly) continue;
                    memberType = fieldSymb.Type;
                }

                if (memberType == null) continue;

                // Pointers are unsupported
                string memberFullTypeName = memberType.ToDisplayString();
                if (memberFullTypeName.Contains("*")) continue;

                // Check if the member type can be serialized itself
                // todo: this is probably irrelevant for the generation of the member reference
                INamedTypeSymbol namedMemberType = memberType as INamedTypeSymbol;
                if (namedMemberType != null)
                {
                    // If the type was marked as non serializable, then skip it.
                    if (HasDontSerialize(namedMemberType)) continue;

                    // If the type wasn't marked as non-serializable, and it cannot be serialized, then
                    // we skip it too, but leave a warning as it should be annotated.
                    if (!IsReflectorBuiltInType(namedMemberType) && !IsReflectorableType(namedMemberType))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(_reflectorMemberWarning, member.Locations.FirstOrDefault()));

                        Console.WriteLine($"Error: Non reflectorable type member in reflectorable class - {memberType.ToDisplayString()} as member {member.ToDisplayString()} of {typ.ToDisplayString()}");
                        continue;
                    }
                }

                result.Add(new ReflectorMemberData()
                {
                    MemberSymbol = member,
                    TypeSymbol = memberType
                });
            }

            return result.ToImmutable();
        }

        private void AddComplexTypeAndAssociatedTypes(SourceProductionContext context, INamedTypeSymbol typ, HashSet<ITypeSymbol> typesToProcess, Dictionary<INamedTypeSymbol, ImmutableArray<ReflectorMemberData>> typeToMembers)
        {
            if (!IsReflectorableType(typ)) return;
            if (IsReflectorBuiltInType(typ)) return;

            if (typ.NullableAnnotation == NullableAnnotation.Annotated)
                typ = (INamedTypeSymbol)typ.WithNullableAnnotation(NullableAnnotation.NotAnnotated);

            string name = typ.ToDisplayString();
            if (name[name.Length - 1] == '?') return;

            if (!typesToProcess.Add(typ)) return;

            Console.WriteLine($"[ReflectorV2] Added type {typ.ToDisplayString()}");

            // Add key and value as associations for dictionaries
            // todo: maybe we should do this for all generic type arguments?
            if (name.StartsWith("System.Collections.Generic.Dictionary"))
            {
                ImmutableArray<ITypeSymbol> keyAndValue = typ.TypeArguments;

                ITypeSymbol keyType = keyAndValue[0];
                INamedTypeSymbol keyTypeNamed = keyType as INamedTypeSymbol;
                if (keyTypeNamed != null)
                {
                    Console.WriteLine($"[ReflectorV2]       Key type found {keyTypeNamed.ToDisplayString()}");
                    AddComplexTypeAndAssociatedTypes(context, keyTypeNamed, typesToProcess, typeToMembers);
                }

                ITypeSymbol valueType = keyAndValue[1];
                INamedTypeSymbol valueTypeNamed = valueType as INamedTypeSymbol;
                if (valueTypeNamed != null)
                {
                    Console.WriteLine($"[ReflectorV2]       Value type found {valueTypeNamed.ToDisplayString()}");
                    AddComplexTypeAndAssociatedTypes(context, valueTypeNamed, typesToProcess, typeToMembers);
                }

                return;
            }

            INamedTypeSymbol baseTyp = typ.BaseType;
            if (baseTyp != null && !IsReflectorBuiltInType(baseTyp)) // Built-in base types are like Object and Valuetype
            {
                Console.WriteLine($"[ReflectorV2]       Base type found {baseTyp.ToDisplayString()}");
                AddComplexTypeAndAssociatedTypes(context, baseTyp, typesToProcess, typeToMembers);
            }

            ImmutableArray<ReflectorMemberData> members = GetReflectorableTypeMembers(context, typ);
            typeToMembers.Add(typ, members); // Cache this for later.

            // Add member types as associations
            foreach (ReflectorMemberData member in members)
            {
                ITypeSymbol memberType = member.TypeSymbol;
                if (memberType.TypeKind == TypeKind.Array && memberType is IArrayTypeSymbol arrayType)
                {
                    ITypeSymbol elementType = arrayType.ElementType;
                    INamedTypeSymbol elementNamedType = elementType as INamedTypeSymbol;
                    if (elementNamedType != null && IsReflectorBuiltInType(elementNamedType)) continue; // Arrays of built in types are also built in

                    if (typesToProcess.Add(memberType))
                    {
                        Console.WriteLine($"[ReflectorV2] Added type {memberType.ToDisplayString()}");

                        // If the array wasnt added - add the element too.
                        Console.WriteLine($"[ReflectorV2]       Element type found {elementNamedType.ToDisplayString()}");
                        AddComplexTypeAndAssociatedTypes(context, elementNamedType, typesToProcess, typeToMembers);
                    }

                    continue;
                }

                INamedTypeSymbol memberTypeNamed = memberType as INamedTypeSymbol;
                if (memberTypeNamed != null)
                {
                    //Console.WriteLine($"[ReflectorV2]     Associated type found {memberType.ToDisplayString()}");
                    AddComplexTypeAndAssociatedTypes(context, memberTypeNamed, typesToProcess, typeToMembers);
                }
                else
                {
                    Console.WriteLine($"[ReflectorV2]     Found unknown associated type {memberType.ToDisplayString()}");
                }
            }
        }

        public static void GenerateHandlerForDictionary(ref SourceProductionContext context, INamedTypeSymbol typ)
        {
            StringBuilder sb = new StringBuilder(1024);

            string fullTypName = typ.ToDisplayString();

            ImmutableArray<ITypeSymbol> keyAndValue = typ.TypeArguments;
            ITypeSymbol keyType = keyAndValue[0];
            ITypeSymbol valueType = keyAndValue[1];
            string keyFullName = keyType.ToDisplayString();
            string valueSafeName = valueType.ToDisplayString();

            string safeNameFull = $"DictionaryOf{GetSafeName(keyFullName)}And{GetSafeName(valueSafeName)}";

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
            sb.AppendLine("");
            sb.AppendLine($"public static class ReflectorData{safeNameFull}");

            sb.AppendLine("{");
            sb.AppendLine("");
            sb.AppendLine("    [ModuleInitializer]");
            sb.AppendLine("    public static void LoadReflector()");
            sb.AppendLine("    {");
            sb.AppendLine($"       ReflectorEngine.RegisterTypeHandler(new DictionaryTypeHandler<{fullTypName}, {keyFullName}, {valueSafeName}>());");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource($"RFLC.{safeNameFull}.g.cs", sb.ToString());
        }

        public static void GenerateHandlerForArrayType(ref SourceProductionContext context, IArrayTypeSymbol typ)
        {
            StringBuilder sb = new StringBuilder(1024);

            string fullTypName = typ.ToDisplayString();

            ITypeSymbol elementType = typ.ElementType;
            string elementFullTypName = elementType.ToDisplayString();
            string safeNameFull = "ArrayOf" + GetSafeName(elementFullTypName);

            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("// Generated by Emotion.SourceGeneration");
            sb.AppendLine();
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using Emotion.Standard.Reflector;");
            sb.AppendLine("using Emotion.Standard.Reflector.Handlers;");
            sb.AppendLine();
            sb.AppendLine($"namespace ReflectorGen;");
            sb.AppendLine("");
            sb.AppendLine($"public static class ReflectorData{safeNameFull}");

            sb.AppendLine("{");
            sb.AppendLine("");
            sb.AppendLine("    [ModuleInitializer]");
            sb.AppendLine("    public static void LoadReflector()");
            sb.AppendLine("    {");
            sb.AppendLine($"       ReflectorEngine.RegisterTypeHandler(new ArrayTypeHandler<{fullTypName}, {elementFullTypName}>());");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource($"RFLC.{safeNameFull}.g.cs", sb.ToString());
        }

        public static void GenerateHandlerForComplexType(ref SourceProductionContext context, INamedTypeSymbol typ, ImmutableArray<ReflectorMemberData> members)
        {
            string fullTypName = typ.ToDisplayString();

            if (fullTypName.StartsWith("System.Collections.Generic.Dictionary"))
            {
                GenerateHandlerForDictionary(ref context, typ);
                return;
            }

            string safeShortName = GetSafeName(typ.Name);
            string safeName = GetSafeName(fullTypName);

            bool partialModeGeneration = IsPartial(typ);
            bool canBeInitialized = CanBeInitialized(typ);

            StringBuilder sb = new StringBuilder(2000);
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("// Generated by Emotion.SourceGeneration");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using Emotion.Standard.Reflector;");
            sb.AppendLine("using Emotion.Standard.Reflector.Handlers;");
            sb.AppendLine();

            if (partialModeGeneration)
            {
                INamespaceSymbol nameSpace = typ.ContainingNamespace;
                sb.AppendLine($"namespace {nameSpace.ToDisplayString()};");
                sb.AppendLine("");

                //if (typ.ContainingType != null)
                //    sb.AppendLine($"public partial class {typ.ContainingType.Name}.{typ.Name}");
                //else
                sb.AppendLine($"public partial class {typ.Name}");
            }
            else
            {
                sb.AppendLine($"namespace ReflectorGen;");
                sb.AppendLine("");
                sb.AppendLine($"public static class ReflectorData{safeName}");
            }

            sb.AppendLine("{");
            sb.AppendLine("");
            sb.AppendLine("    [ModuleInitializer]");
            sb.AppendLine("    public static void LoadReflector()");
            sb.AppendLine("    {");
            sb.AppendLine($"       ReflectorEngine.RegisterTypeHandler(new ComplexTypeHandler<{fullTypName}>(");
            if (canBeInitialized)
            {
                if (typ.IsTupleType)
                    sb.AppendLine($"           () => new ValueTuple{fullTypName.Replace("(", "<").Replace(")", ">")}(),");
                else
                    sb.AppendLine($"           () => new {fullTypName}(),");
            }
            else
            {
                sb.AppendLine($"           null,");
            }
            sb.AppendLine($"           \"{safeShortName}\",");
            sb.AppendLine($"           new ComplexTypeHandlerMember<{fullTypName}>[] {{");

            foreach (ReflectorMemberData memberDesc in members)
            {
                ITypeSymbol memberType = memberDesc.TypeSymbol;
                ISymbol memberSymbol = memberDesc.MemberSymbol;

                // Can't use nullable reference types in a typeof, so strip the marker from the name.
                string memberFullTypeName = memberType.ToDisplayString();
                if (memberType.IsReferenceType && memberFullTypeName[memberFullTypeName.Length - 1] == '?')
                    memberFullTypeName = memberFullTypeName.Substring(0, memberFullTypeName.Length - 1);

                string memberName = memberSymbol.Name;

                sb.AppendLine($"               new ComplexTypeHandlerMember<{fullTypName}, {memberFullTypeName}>(\"{memberName}\", (p, v) => p.{memberName} = v, (p) => p.{memberName})");
                sb.AppendLine("               {");

                // Generate attributes
                ImmutableArray<AttributeData> memberAttributes = memberSymbol.GetAttributes();
                if (memberAttributes.Length > 0)
                {
                    sb.AppendLine("                   Attributes = new Attribute[] {");
                    foreach (AttributeData attribute in memberAttributes)
                    {
                        INamedTypeSymbol clazz = attribute.AttributeClass;
                        if (clazz.Name != "VertexAttributeAttribute" &&
                            clazz.Name != "DontShowInEditorAttribute") continue; // todo

                        sb.AppendLine($"                       {GenerateAttributeDeclaration(attribute)},");
                    }
                    sb.AppendLine("                   },");
                }

                sb.AppendLine("               },");
            }
            sb.AppendLine("           }");
            sb.AppendLine("       ));");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource($"RFLC.{safeName}.g.cs", sb.ToString());
        }
    }
}