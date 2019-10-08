#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CSharp.RuntimeBinder;

#endregion

namespace Emotion.Scripting
{
    /// <summary>
    /// A C# scripting engine using Roslyn.
    /// </summary>
    public static class CSharpScriptEngine
    {
        /// <summary>
        /// Run a string as CSharp code and return the output.
        /// </summary>
        /// <param name="script">The script to run.</param>
        /// <param name="args">Arguments to pass to it.</param>
        /// <returns>The return value.</returns>
        public static Task<object> RunScript(string script, params object[] args)
        {
            var runtimeAsset = new CSharpScriptAsset(script);
            return RunScript(runtimeAsset, args).ContinueWith(r =>
            {
                runtimeAsset.Dispose();
                return r.Result;
            });
        }

        /// <summary>
        /// Run a CSharpScriptAsset and return the output.
        /// </summary>
        /// <param name="scriptAsset">The asset to run.</param>
        /// <param name="args">Arguments to pass to it.</param>
        /// <returns>The return value.</returns>
        public static Task<object> RunScript(CSharpScriptAsset scriptAsset, params object[] args)
        {
            // No loaded assembly to run.
            if (scriptAsset.Assembly == null) return Task.FromResult<object>(null);

            // Find the entry point.
            MethodInfo entry = scriptAsset.Assembly.EntryPoint;
            MethodInfo customEntry = entry?.DeclaringType?.GetMethod("ScriptMain");
            if (customEntry != null) entry = customEntry;

            // Run the code.
            if (entry != null) return Task.Run(() => entry.Invoke(null, entry.GetParameters().Length == 0 ? null : args));
            Engine.Log.Warning("Script has no entry point.", MessageSource.ScriptingEngine);
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Compiles the provided C# code.
        /// </summary>
        /// <param name="sourceCode">The raw source to compile.</param>
        /// <returns>A stream of compiled bytecode.</returns>
        public static MemoryStream CompileCode(string sourceCode)
        {
            SourceText codeString = SourceText.From(sourceCode);
            CSharpParseOptions options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
            SyntaxTree parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

            Assembly entryAsm = Assembly.GetEntryAssembly();
            var references = new MetadataReference[]
            {
                // System references.
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(AssemblyTargetedPatchBandAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(CSharpArgumentInfo).Assembly.Location),

                // Emotion references.
                MetadataReference.CreateFromFile(typeof(Engine).Assembly.Location),

                // Game references.
                MetadataReference.CreateFromFile(Assembly.GetCallingAssembly().Location),
                MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location),
                entryAsm != null ? MetadataReference.CreateFromFile(entryAsm.Location) : null
            };

            CSharpCompilation compilation = CSharpCompilation.Create($"{Guid.NewGuid().ToString()}.dll",
                new[] {parsedSyntaxTree},
                references,
                new CSharpCompilationOptions(OutputKind.WindowsApplication,
                    optimizationLevel: Engine.Configuration.DebugMode ? OptimizationLevel.Debug : OptimizationLevel.Release,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default)
            );

            var ilStream = new MemoryStream();
            EmitResult result = compilation.Emit(ilStream);

            if (!result.Success)
            {
                Engine.Log.Warning("Failed to compile script.", MessageSource.ScriptingEngine);
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
                foreach (Diagnostic diagnostic in failures)
                {
                    Engine.Log.Warning($"Error {diagnostic.Id} - {diagnostic.GetMessage()}", MessageSource.ScriptingEngine);
                }

                return null;
            }

            ilStream.Seek(0, SeekOrigin.Begin);
            return ilStream;
        }
    }
}