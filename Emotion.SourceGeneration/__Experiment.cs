using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace Emotion.SourceGeneration
{
    internal class __Experiment
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

        public class NamedTypeSymbolCompare : IEqualityComparer<INamedTypeSymbol>
        {
            public bool Equals(INamedTypeSymbol x, INamedTypeSymbol y)
            {
                return false;
            }

            public int GetHashCode(INamedTypeSymbol obj)
            {
                return 0;
            }
        }

        public static IEnumerable<INamedTypeSymbol> FilterTypesToGenerateFor(INamespaceSymbol space, string mainNamespace, bool includeTypes = true)
        {
            ImmutableArray<INamedTypeSymbol> types = space.GetTypeMembers();
            string fullSpaceName = space.ToDisplayString();

            bool includeTypesFromNamespace = includeTypes && !_excludedNamespacesTypes.Contains(fullSpaceName);
            bool includeSubSpaces = includeTypes && !_excludedNamespacesSubSpaces.Contains(fullSpaceName);
            if (!includeSubSpaces) includeTypesFromNamespace = false;

            if (mainNamespace == "Emotion" && _forceIncludeSubspaces.Contains(fullSpaceName))
            {
                includeTypesFromNamespace = true;
                includeSubSpaces = true;
            }

            IEnumerable<INamedTypeSymbol> typesToGenerateFor = space.GetNamespaceMembers().SelectMany(x => FilterTypesToGenerateFor(x, mainNamespace, includeSubSpaces));
            if (includeTypesFromNamespace)
            {
                typesToGenerateFor = typesToGenerateFor.Union(types, new NamedTypeSymbolCompare());
            }

            return typesToGenerateFor;
        }
    }
}

//IncrementalValuesProvider<INamedTypeSymbol> syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
//    predicate: ReflectorVer2_SyntaxNodeIsValid,
//    transform: ReflectorVer2_TransformNode
//)
//.Where(node => node != null);
//context.RegisterSourceOutput(syntaxProvider, ReflectVer2_ProcessGenericSpecialization);

//IncrementalValuesProvider<INamedTypeSymbol> genericTypeMatches = context.SyntaxProvider
//    .CreateSyntaxProvider(
//        predicate: (node, token) => node is GenericNameSyntax,
//        transform: (ctx, token) =>
//        {
//            var genericNameSyntax = (GenericNameSyntax)ctx.Node;
//            var symbolInfo = ctx.SemanticModel.GetSymbolInfo(genericNameSyntax, token);
//            if (symbolInfo.Symbol is INamedTypeSymbol namedType && namedType.IsGenericType)
//                return namedType;
//            return null;
//        }
//    ).Where(symbol => symbol != null);
//context.RegisterSourceOutput(genericTypeMatches, ReflectVer2_ProcessGenericSpecialization);

//private static HashSet<INamedTypeSymbol> _uniqueSpecializations = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

//public static bool ReflectorVer2_SyntaxNodeIsValid(SyntaxNode node, CancellationToken _)
//{
//    if (node is InterfaceDeclarationSyntax)
//        return false;

//    if (node is GenericNameSyntax genericSpecialization)
//    {
//        bool a = true;
//    }

//    if (node is TypeDeclarationSyntax typeDeclaration)
//        return typeDeclaration.BaseList?.Types.Count > 0;

//    return false;
//}

//public static INamedTypeSymbol ReflectorVer2_TransformNode(GeneratorSyntaxContext context, CancellationToken _)
//{
//    INamedTypeSymbol markerInterface = context.SemanticModel.Compilation.Assembly.GetTypeByMetadataName($"Emotion.Standard.Reflector.IReflectorSupportingType");

//    var typeDeclaration = (TypeDeclarationSyntax)context.Node;
//    var semanticModel = context.SemanticModel;
//    var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;

//    if (typeSymbol.AllInterfaces.Contains(markerInterface))
//        return typeSymbol;

//    return null;
//}

//public static void ReflectVer2_ProcessGenericSpecialization(SourceProductionContext context, INamedTypeSymbol symbol)
//{
//    if (!_uniqueSpecializations.Add(symbol)) return;

//    Console.WriteLine($"[Reflector V2] Type found: {symbol.ToDisplayString()}");
//    context.AddSource($"{symbol.ToDisplayString()}.generated.cs", "");
//}
