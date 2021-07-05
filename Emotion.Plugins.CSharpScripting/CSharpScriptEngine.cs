#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Utility;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

#endregion

namespace Emotion.Plugins.CSharpScripting
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

        private static HashSet<string> _refAssemblyNames;
        private static MetadataReference[] _scriptingReferences;
        private static object _lockPopulate = new object();

        private static void PopulateReferences()
        {
            lock (_lockPopulate)
            {
                // Check if built while waiting on lock.
                if (_scriptingReferences != null) return;

                var entryAsm = Assembly.GetEntryAssembly();
                _refAssemblyNames = new HashSet<string>
                {
                    // System references.
                    typeof(object).Assembly.Location,
                    typeof(Console).Assembly.Location,

                    // Emotion references.
                    typeof(Engine).Assembly.Location,

                    // Game references
                    Assembly.GetCallingAssembly().Location,
                    Assembly.GetExecutingAssembly().Location,
                    entryAsm != null ? entryAsm.Location : null
                };

                foreach (Assembly associatedAssembly in Helpers.AssociatedAssemblies)
                {
                    _refAssemblyNames.Add(associatedAssembly.Location);
                }

                AssemblyName[] callerReferences = Assembly.GetCallingAssembly().GetReferencedAssemblies();
                foreach (AssemblyName refAssembly in callerReferences)
                {
                    _refAssemblyNames.Add(Assembly.Load(refAssembly).Location);
                }

                _scriptingReferences = new MetadataReference[_refAssemblyNames.Count];
                var idx = 0;
                foreach (string location in _refAssemblyNames)
                {
                    _scriptingReferences[idx] = MetadataReference.CreateFromFile(location);
                    idx++;
                }
            }
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

            if (_scriptingReferences == null) PopulateReferences();

            var compilation = CSharpCompilation.Create($"{Guid.NewGuid()}.dll",
                new[] { parsedSyntaxTree },
                _scriptingReferences,
                new CSharpCompilationOptions(
                    OutputKind.ConsoleApplication, // Mark as console application so the entry point is assigned to Main. Libraries dont have entry points.
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

            // Print warnings and info texts in debug mode.
            if (Engine.Configuration.DebugMode)
            {
                foreach (Diagnostic diagnostic in result.Diagnostics)
                {
                    Engine.Log.Warning($"{diagnostic.Id} - {diagnostic.GetMessage()}", MessageSource.ScriptingEngine);
                }
            }

            ilStream.Seek(0, SeekOrigin.Begin);
            return ilStream;
        }
    }
}