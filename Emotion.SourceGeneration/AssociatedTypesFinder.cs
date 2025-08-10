using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using static Emotion.SourceGeneration.Helpers;
using static SourceGenerator.Generator;

namespace Emotion.SourceGeneration
{
    /// <summary>
    /// Reflector only adds types from the compiled assembly, however those types may reference types from other
    /// assemblies such as via members or generic arguments. This class finds and adds these types.
    /// </summary>
    public static class AssociatedTypesFinder
    {
        public static void AddTypeAndAssociatedTypes(
            SourceProductionContext context,
            ITypeSymbol typ,
            HashSet<ITypeSymbol> typesToProcess,
            Dictionary<INamedTypeSymbol, ImmutableArray<ReflectorMemberData>> typeToMembers,
            string reason = null)
        {
            if (typ is INamedTypeSymbol namedTypProcessable)
            {
                if (!IsReflectorableType(namedTypProcessable)) return;
                if (IsReflectorBuiltInType(namedTypProcessable)) return;
            }

            INamespaceSymbol nameSpace = typ.ContainingNamespace;
            if (nameSpace != null)
            {
                string displayStr = nameSpace.ToDisplayString();
                if (IsExcludedNamespace(displayStr, true)) return;
            }

            // todo: gotta figure out what to do with nullables
            if (typ.NullableAnnotation == NullableAnnotation.Annotated)
                typ = typ.WithNullableAnnotation(NullableAnnotation.NotAnnotated);

            string name = typ.ToDisplayString();
            if (name[name.Length - 1] == '?') return;

            // Type is already being processed.
            if (!typesToProcess.Add(typ)) return;

            if (reason == null)
                Console.WriteLine($"[ReflectorV2] Added type {typ.ToDisplayString()}");
            else
                Console.WriteLine($"[ReflectorV2]       Added {reason} type {typ.ToDisplayString()}");

            // Array element type
            if (typ.TypeKind == TypeKind.Array && typ is IArrayTypeSymbol arrayType)
            {
                ITypeSymbol elementType = arrayType.ElementType;
                AddTypeAndAssociatedTypes(context, elementType, typesToProcess, typeToMembers, "element type");
            }

            // Members
            if (typ is INamedTypeSymbol namedTyp)
            {
                // Inheriting type
                INamedTypeSymbol baseTyp = typ.BaseType;
                if (baseTyp != null)
                    AddTypeAndAssociatedTypes(context, baseTyp, typesToProcess, typeToMembers, "base");

                // Generic arguments
                if (namedTyp.IsGenericType)
                {
                    foreach (ITypeSymbol genericTyp in namedTyp.TypeArguments)
                    {
                        if (genericTyp is INamedTypeSymbol genericNamedTyp)
                            AddTypeAndAssociatedTypes(context, genericNamedTyp, typesToProcess, typeToMembers, "generic argument");
                    }
                }

                ImmutableArray<ReflectorMemberData> members = GetReflectorableTypeMembers(context, namedTyp);
                typeToMembers.Add(namedTyp, members); // Cache this for generation later.

                // Add member types as associations
                foreach (ReflectorMemberData member in members)
                {
                    ISymbol memberSymbol = member.MemberSymbol;
                    string memberName = memberSymbol?.Name ?? string.Empty;

                    ITypeSymbol memberType = member.TypeSymbol;
                    AddTypeAndAssociatedTypes(context, memberType, typesToProcess, typeToMembers, $"member ({memberName})");
                }
            }

            // DONE!
        }
    }
}
