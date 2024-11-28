using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

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
