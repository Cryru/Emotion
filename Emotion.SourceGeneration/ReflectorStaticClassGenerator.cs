﻿using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;
using static SourceGenerator.Generator;
using static Emotion.SourceGeneration.Helpers;
using System;

namespace Emotion.SourceGeneration
{
    /// <summary>
    /// Generates Emotion Reflector support for static classes
    /// which are marked with Emotion.Standard.Reflector.ReflectorStaticClassSupportAttribute.
    /// </summary>
    public static class ReflectorStaticClassGenerator
    {
        public static bool Run(ref SourceProductionContext context, INamedTypeSymbol typ)
        {
            // Filter out static types from the main generator
            if (!typ.IsStatic) return false;

            // Static types with the attribute will have a handler for their members generated.
            if (HasAttribute(typ.GetAttributes(), "ReflectorStaticClassSupportAttribute"))
            {
                Console.WriteLine($"[ReflectorV2-Static] Generating handler for {typ.ToDisplayString()}.");

                ImmutableArray<ReflectorMemberData> members = GetReflectorableTypeMembers(context, typ, true);
                GenerateHandlerForStaticComplexType(ref context, typ, members);
            }

            return true;
        }

        private static void GenerateHandlerForStaticComplexType(ref SourceProductionContext context, INamedTypeSymbol typ, ImmutableArray<ReflectorMemberData> members)
        {
            string fullTypName = typ.ToDisplayString();

            string safeShortName = GetSafeName(typ.Name);
            string safeName = GetSafeName(fullTypName);

            StringBuilder sb = new StringBuilder(2000);
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("// Generated by Emotion.SourceGeneration");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using Emotion.Standard.Reflector;");
            sb.AppendLine("using Emotion.Standard.Reflector.Handlers;");
            sb.AppendLine("using Emotion.Standard.Reflector.Handlers.Base;");
            sb.AppendLine();

            sb.AppendLine($"namespace ReflectorGen;");
            sb.AppendLine("");
            sb.AppendLine($"[System.CodeDom.Compiler.GeneratedCode(\"Emotion.SourceGeneration - Reflector\", \"2.0\")]");
            sb.AppendLine($"public static class ReflectorData{safeName}");

            sb.AppendLine("{");
            sb.AppendLine("");
            sb.AppendLine("    [ModuleInitializer]");
            sb.AppendLine("    public static void LoadReflector()");
            sb.AppendLine("    {");
            sb.AppendLine($"       ReflectorEngine.RegisterTypeHandler(new StaticComplexTypeHandler(");
            sb.AppendLine($"           typeof({fullTypName}),");
            sb.AppendLine($"           \"{safeShortName}\",");
            sb.AppendLine($"           new ComplexTypeHandlerMemberBase[] {{");

            foreach (ReflectorMemberData memberDesc in members)
            {
                ITypeSymbol memberType = memberDesc.TypeSymbol;
                ISymbol memberSymbol = memberDesc.MemberSymbol;

                // Can't use nullable reference types in a typeof, so strip the marker from the name.
                string memberFullTypeName = memberType.ToDisplayString();
                if (memberType.IsReferenceType && memberFullTypeName[memberFullTypeName.Length - 1] == '?')
                    memberFullTypeName = memberFullTypeName.Substring(0, memberFullTypeName.Length - 1);

                string memberName = memberSymbol.Name;

                sb.AppendLine($"               new ComplexTypeHandlerMember<object, {memberFullTypeName}>(\"{memberName}\", (ref object p, {memberFullTypeName} v) => {fullTypName}.{memberName} = v, (p) => {fullTypName}.{memberName})");
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
