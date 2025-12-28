using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using static SourceGenerator.Generator;
using MemberMap = System.Collections.Generic.Dictionary<Microsoft.CodeAnalysis.INamedTypeSymbol, System.Collections.Immutable.ImmutableArray<SourceGenerator.Generator.ReflectorMemberData>>;

namespace Emotion.SourceGeneration
{
    [Generator]
    public class ReflectorMain : IIncrementalGenerator
    {
        public static Compilation CurrentCompilation = null;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            return;

            // Find all generic calls of Reflector functions
            IncrementalValuesProvider<IMethodSymbol> genericCalls = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsGenericInvocation,
                transform: FilterReflectorCalls
            ).Where(m => m != null);

            // Get all class declarations - needed in order to find inheriting types
            IncrementalValuesProvider<ITypeSymbol> allClasses = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (n, _) => n is ClassDeclarationSyntax,
                transform: (ctx, _) => (ITypeSymbol)ctx.SemanticModel.GetDeclaredSymbol(ctx.Node)
            )
            .Where(c => c != null);

            // Flatten the specializations of Reflector generic calls.
            var typeToMembers = new MemberMap(SymbolEqualityComparer.Default);
            IncrementalValueProvider<ImmutableArray<ITypeSymbol>> specializations = genericCalls
              .Select((method, _) => method.TypeArguments)
              .Collect()
              .Combine(allClasses.Collect())
              .Select((pair, _) =>
                  {
                      try
                      {
                          (ImmutableArray<ImmutableArray<ITypeSymbol>> arrays, ImmutableArray<ITypeSymbol> projectClasses) = pair;
                          HashSet<ITypeSymbol> processed = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
                          HashSet<ITypeSymbol> added = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
                          return arrays
                            .SelectMany(arr => arr.SelectMany(x => AddAssociatedTypes(x, projectClasses, "used in generic method", 0, processed, added, typeToMembers)))
                            .Distinct<ITypeSymbol>(SymbolEqualityComparer.Default)
                            .ToImmutableArray();
                      }
                      catch (Exception ex)
                      {
                          Console.WriteLine(ex.ToString());
                          return Array.Empty<ITypeSymbol>().ToImmutableArray();
                      }
                  });


            // Find all member types and associated types - we will create handlers for those
            context.RegisterSourceOutput(specializations, (sourceProductionContext, typeList) =>
            {
                try
                {
                    foreach (ITypeSymbol type in typeList)
                    {
                        INamedTypeSymbol typeNamed = type as INamedTypeSymbol;

                        // Some types are non-named (arrays)
                        if (ReflectorEnumerableGenerator.Run(ref sourceProductionContext, type)) continue;

                        if (typeNamed == null) continue;

                        // Some types are either handled by other generators or have additional generation.
                        GameDataEditorSupportGenerator.Run(ref sourceProductionContext, typeNamed);
                        StructPerMemberHelpersGenerator.Run(ref sourceProductionContext, typeNamed);
                        if (ReflectorEnumGenerator.Run(ref sourceProductionContext, typeNamed)) continue;

                        // Get member list of complex type
                        if (!typeToMembers.TryGetValue(typeNamed, out ImmutableArray<ReflectorMemberData> members))
                            members = GetReflectorableTypeMembers(sourceProductionContext, typeNamed);

                        Console.WriteLine($"[Emotion SourceGen] Generating handler for {type.ToDisplayString()}.");
                        GenerateHandlerForComplexType(ref sourceProductionContext, typeNamed, members);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                Console.WriteLine("Done!");
            });
        }

        #region Config

        private static readonly HashSet<string> _reflectorMethods = new HashSet<string>()
        {
            //"GetDescendantsOf",
            "GetTypeInfo"
        };

        private static readonly HashSet<string> _excludeTypes = new HashSet<string>()
        {
            "System.Array",
            "System.IEquatable",
            "System.Collections.Generic.IEnumerable",
            "System.Collections.IEnumerable",
            "System.Delegate",
            "System.MulticastDelegate",
            "System.ICloneable",
            "System.Runtime.Serialization.ISerializable",
            "System.Exception",
            "System.ISpanFormattable",
            "System.IFormattable",
            "System.IConvertible",
            "System.IComparable",
            "System.Collections.IStructuralComparable",
            "System.IDisposable",
            "System.Func",
            "System.Action",
            "System.Collections.Generic.ICollection",
            "System.Collections.Generic.IReadOnlyList",
            "System.Collections.Generic.IReadOnlyCollection",
            "System.Collections.Generic.IReadOnlyDictionary",
            "System.Collections.Generic.ISet",
            "System.Collections.Generic.IReadOnlySet",
            "System.Collections.Generic.IList",
            "System.Collections.IList",
            "System.Collections.ICollection",
            "System.Attribute",
            "System.Collections.IEnumerator",
            "System.IAsyncResult",
            "Emotion.Standard.Reflector",
            "Emotion.Core.Systems.IO.AssetOwner",
            "System.Threading",
            "OpenGL",
            "Emotion.Core.Utility.Coroutines",
            "Emotion.Core.Systems.JobSystem",
            "Emotion.Core.Utility.Threading",
            "Emotion.Core.Systems.Audio",
            "StbTrueTypeSharp",
            "Emotion.Core.Platform",
            "System.Collections.Generic.Stack",
            "System.Collections.Generic.IDictionary",
            "System.Reflection",
            "System.Type",
            "Emotion.Graphics.Memory"
        };

        #endregion

        #region Helpers

        private static bool IsGenericInvocation(SyntaxNode node, CancellationToken _)
        {
            if (node is InvocationExpressionSyntax inv)
            {
                if (inv.Expression is GenericNameSyntax) return true; // GenericFunc<T>()
                if (inv.Expression is MemberAccessExpressionSyntax member && member.Name is GenericNameSyntax) return true; // Class.GenericFunc<T>()
            }
            return false;
        }

        private static IMethodSymbol FilterReflectorCalls(GeneratorSyntaxContext ctx, CancellationToken _)
        {
            InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)ctx.Node;
            IMethodSymbol symbol = ctx.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (symbol == null)
                return null;

            // Filter reflector generic methods
            if (symbol.IsGenericMethod && _reflectorMethods.Contains(symbol.Name))
                return symbol;

            return null;
        }

        private static IEnumerable<ITypeSymbol> AddAssociatedTypes(
            ITypeSymbol type,
            ImmutableArray<ITypeSymbol> allClasses,
            string reason,
            int depth,
            HashSet<ITypeSymbol> dedupe,
            HashSet<ITypeSymbol> added,
            MemberMap memberMap
        )
        {
            // Filter out
            // ----------
            if (type == null) yield break;
            if (dedupe.Contains(type)) yield break;
            dedupe.Add(type);

            if (Helpers.IsReflectorBuiltInType(type)) yield break;
            if (type.DeclaredAccessibility != Accessibility.Public) yield break;

            string name = type.ToString();

            // No pointers
            if (name.EndsWith("*")) yield break;

            // No excluded types/namespaces
            foreach (string exclusionName in _excludeTypes)
            {
                if (name.StartsWith(exclusionName)) yield break;
            }

            INamedTypeSymbol complexType = type as INamedTypeSymbol;

            // todo: safe name, get member names etc
            if (type.IsTupleType ||
                name.Contains("System.Tuple") ||
                (complexType != null && (complexType.OriginalDefinition.IsTupleType || complexType.TupleUnderlyingType != null))
                ) yield break;

            // ----------
            // Advanced filtering
            // ----------

            // Type is contained in another type.
            if (type.ContainingType != null)
            {
                INamedTypeSymbol containType = type.ContainingType;
                foreach (ITypeSymbol containAss in AddAssociatedTypes(containType, allClasses, "contains", depth + 1, dedupe, added, memberMap))
                {
                    yield return containAss;
                }

                // No use in adding an array type whose containing type is not eligible.
                if (!added.Contains(containType)) yield break;
            }

            // Array element type
            if (type.TypeKind == TypeKind.Array && type is IArrayTypeSymbol arrayType)
            {
                ITypeSymbol elementType = arrayType.ElementType;
                foreach (ITypeSymbol elementAss in AddAssociatedTypes(elementType, allClasses, "element type", depth + 1, dedupe, added, memberMap))
                {
                    yield return elementAss;
                }

                // No use in adding an array type whose element is not eligible.
                if (!added.Contains(elementType)) yield break;
            }

            // Complex type generics
            if (complexType != null && complexType.IsGenericType)
            {
                // No non-specified generics
                bool allGenericsSpecified = true;
                foreach (ITypeSymbol genericTyp in complexType.TypeArguments)
                {
                    if (genericTyp.TypeKind == TypeKind.TypeParameter)
                    {
                        allGenericsSpecified = false;
                        break;
                    }
                }
                if (!allGenericsSpecified) yield break;

                bool allGenericsAdded = true;
                foreach (ITypeSymbol genericTyp in complexType.TypeArguments)
                {
                    foreach (ITypeSymbol genericAss in AddAssociatedTypes(genericTyp, allClasses, "generic argument", depth + 1, dedupe, added, memberMap))
                    {
                        yield return genericAss;
                    }
                    allGenericsAdded = allGenericsAdded && added.Contains(genericTyp);
                }

                // No use in adding a type with a generic argument that couldnt be added
                if (!allGenericsAdded) yield break;
            }

            // ----------

            Console.WriteLine($"[Emotion SourceGen] {new string('|', depth)}Adding type because {reason} :: {name}");
            yield return type; // yield self too :)
            added.Add(type);

            foreach (ITypeSymbol baseAss in AddAssociatedTypes(type.BaseType, allClasses, "base", depth + 1, dedupe, added, memberMap))
            {
                yield return baseAss;
            }

            foreach (ITypeSymbol i in type.AllInterfaces)
            {
                foreach (ITypeSymbol iAss in AddAssociatedTypes(i, allClasses, "interface", depth + 1, dedupe, added, memberMap))
                {
                    yield return iAss;
                }
            }

            // Add classes that inherit it
            foreach (ITypeSymbol cl in allClasses)
            {
                if (!SymbolEqualityComparer.Default.Equals(cl.BaseType, type) && !cl.AllInterfaces.Contains(type, SymbolEqualityComparer.Default))
                    continue;

                foreach (ITypeSymbol clAss in AddAssociatedTypes(cl, allClasses, "inheritance", depth + 1, dedupe, added, memberMap))
                {
                    yield return clAss;
                }
            }

            // Complex type
            if (complexType != null)
            {
                // Members
                List<ReflectorMemberData> memberList = new List<ReflectorMemberData>();
                ImmutableArray<ISymbol> members = type.GetMembers();
                foreach (ISymbol member in members)
                {
                    // Exclude privates etc.
                    if (member.DeclaredAccessibility != Accessibility.Public) continue;

                    // Exclude function members and other weird kinds
                    if (member.Kind != SymbolKind.Field && member.Kind != SymbolKind.Property) continue;

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

                    foreach (ITypeSymbol memberAss in AddAssociatedTypes(memberType, allClasses, "member", depth + 1, dedupe, added, memberMap))
                    {
                        yield return memberAss;
                    }

                    if (added.Contains(memberType))
                    {
                        memberList.Add(new ReflectorMemberData()
                        {
                            MemberSymbol = member,
                            TypeSymbol = memberType
                        });
                    }
                }

                memberMap.Add(complexType, memberList.ToImmutableArray());
            }


        }

        #endregion

        #region Generators



        #endregion
    }
}
