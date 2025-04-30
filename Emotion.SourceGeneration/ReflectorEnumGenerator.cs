using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Text;
using static Emotion.SourceGeneration.Helpers;
using static SourceGenerator.Generator;

namespace Emotion.SourceGeneration
{
    /// <summary>
    /// Generates supporting code for the game data editor.
    /// </summary>
    public static class ReflectorEnumGenerator
    {
        public static bool Run(ref SourceProductionContext context, INamedTypeSymbol typ)
        {
            if (typ.BaseType != null && typ.BaseType.Name == "Enum")
            {
                Console.WriteLine($"[ReflectorV2-EnumGenerator] Generating handler for {typ.ToDisplayString()}.");
                GenerateEnumHandler(ref context, typ);
                return true;
            }
            return false;
        }

        // Generates a Undefined{GameDataType}Class for each game data type. This class is used as a placeholder for new
        // objects created by the editor, until the code is hot reloaded. It inherits the game data type so it can have the
        // same properties and be edited freely.
        private static void GenerateEnumHandler(ref SourceProductionContext context, INamedTypeSymbol typ)
        {
            ImmutableArray<ISymbol> members = typ.GetMembers();

            string fullTypName = typ.ToDisplayString();
            INamedTypeSymbol underlyingType = typ.EnumUnderlyingType;
            string underlyingTypeName = underlyingType.Name;

            string safeShortName = GetSafeName(typ.Name);
            string safeName = GetSafeName(fullTypName);

            StringBuilder sb = new StringBuilder(2000);
            WriteFileHeader(sb);

            sb.AppendLine($"namespace ReflectorGen;");
            sb.AppendLine("");
            sb.AppendLine($"public static class ReflectorData{safeName}");
            sb.AppendLine("{");
            sb.AppendLine("");
            sb.AppendLine("    [ModuleInitializer]");
            sb.AppendLine("    public static void LoadReflector()");
            sb.AppendLine("    {");
            sb.AppendLine($"       ReflectorEngine.RegisterTypeHandler(new EnumTypeHandler<{fullTypName}, {underlyingTypeName}>(");
            sb.AppendLine($"            new Dictionary<string, ({fullTypName}, {underlyingTypeName})>(StringComparer.OrdinalIgnoreCase)");
            sb.AppendLine($"            {{");
            foreach (ISymbol member in members)
            {
                if (!member.IsStatic) continue;
                string memberName = $"global::{fullTypName}.{member.Name}";
                sb.AppendLine($"                 {{\"{member.Name}\", ({memberName}, ({underlyingTypeName}) {memberName})}},");
            }
            sb.AppendLine($"            }}");
            
            sb.AppendLine("       ));");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource($"RFLC.{safeName}.g.cs", sb.ToString());
        }
    }
}
