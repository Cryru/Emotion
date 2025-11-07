using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using static SourceGenerator.Generator;

namespace Emotion.SourceGeneration
{
    [Generator]
    public class ReflectorMain : IIncrementalGenerator
    {
        public static Compilation CurrentCompilation = null;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Find all generic calls of Reflector functions
            IncrementalValuesProvider<IMethodSymbol> genericCalls = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsGenericInvocation,
                transform: FilterReflectorCalls
            ).Where(m => m != null);

            // Flatten the specializations of Reflector generic calls.
            IncrementalValueProvider<ImmutableArray<ITypeSymbol>> specializations = genericCalls
              .Select((method, _) => method.TypeArguments)
              .Collect()
              .Select((arrays, _) => arrays
                .SelectMany(x => x)
                .Distinct<ITypeSymbol>(SymbolEqualityComparer.Default)
                .ToImmutableArray()
              );

            // Find all member types and associated types - we will create handlers for those
            HashSet<ITypeSymbol> typesToReflector = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            Dictionary<INamedTypeSymbol, ImmutableArray<ReflectorMemberData>> typeToMembers = new Dictionary<INamedTypeSymbol, ImmutableArray<ReflectorMemberData>>(SymbolEqualityComparer.Default);
            context.RegisterSourceOutput(specializations, (sourceProductionContext, typeList) =>
            {
                foreach (ITypeSymbol typUsed in typeList)
                {
                    AssociatedTypesFinder.AddTypeAndAssociatedTypes(sourceProductionContext, typUsed, typesToReflector, typeToMembers);
                }
            });
        }

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

        private static HashSet<string> _reflectorMethods = new HashSet<string>()
        {
            "GetTypesDescendedFrom"
        };

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

        #endregion
    }
}
