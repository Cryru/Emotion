#nullable enable

#if DEBUG
using Emotion;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;


namespace Emotion.Editor.RuntimeCodeGen;

public enum ReloadType
{
    None,
    VisualStudioHotReload,
    RoslynHotReload
}

/// <summary>
/// Allows for programatic hot reloading for developers using Visual Studio and team members who don't have Visual Studio installed.
/// Debug only builds of course.
/// </summary>
public static class EditorReload
{
    public static ReloadType Type { get; private set; }

    internal static void Init()
    {
        CheckReloadType();
        Engine.Log.ONE_Info(nameof(EditorReload), $"Detected hot reload type: {Type}");
    }

    public static bool TryReload()
    {
        try
        {
            switch (Type)
            {
                case ReloadType.VisualStudioHotReload:
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
                    _dte!.ExecuteCommand("Debug.ApplyCodeChanges");
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                    Engine.Log.ONE_Info(nameof(EditorReload), $"Reloaded the whole assembly via Visual Studio");
                    return true;
                case ReloadType.RoslynHotReload:
                    Roslyn_ReloadModifiedFiles();
                    return true;
                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            Engine.Log.ONE_Error(nameof(EditorReload), $"Failed to hot reload - {ex}");
            return false;
        }
    }

    private static void CheckReloadType()
    {
        // If the debugger is attached we can only do Visual Studio reload
        // We could detach-attach the debugger, but we can't detach it if any hot reloads have gone through (todo: test)
        if (Debugger.IsAttached)
        {
            Type = VS_SetupHotReload() ? ReloadType.VisualStudioHotReload : ReloadType.None;
            return;
        }

        // If not ran from visual studio we can still hot reload via Roslyn
        Type = Roslyn_SetupHotReload() ? ReloadType.RoslynHotReload : ReloadType.None;
    }

    #region Visual Studio Reload

    private static dynamic? _dte;

    private static bool VS_SetupHotReload()
    {
        dynamic? dte = GetDTE();
        if (dte == null) return false;
        _dte = dte;
        return true;
    }

    [DllImport("ole32.dll")] private static extern int GetRunningObjectTable(int r, out IRunningObjectTable pprot);
    [DllImport("ole32.dll")] private static extern int CreateBindCtx(int r, out IBindCtx ppbc);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Trimming", "IL2050:Correctness of COM interop cannot be guaranteed after trimming. Interfaces and interface members might be removed.", Justification = "<Pending>")]
    private static dynamic? GetDTE()
    {
        IRunningObjectTable rot;
        int _ = GetRunningObjectTable(0, out rot);
        rot.EnumRunning(out var enumerator);

        IMoniker[] monikers = new IMoniker[1];
        while (enumerator.Next(1, monikers, IntPtr.Zero) == 0)
        {
            _ = CreateBindCtx(0, out var ctx);
            monikers[0].GetDisplayName(ctx, null, out var name);

            if (name.StartsWith("!VisualStudio.DTE"))
            {
                rot.GetObject(monikers[0], out var obj);
                return obj as dynamic;
            }
        }
        return null;
    }

    #endregion

    #region Roslyn HotReload

    private static CSharpCompilation _compilation = null!;
    private static Assembly _assembly = null!;
    private static EmitBaseline _baseline = null!;
    private static Dictionary<string, DateTime> _modifiedSourceLast = new();
    private static CSharpParseOptions _parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);

    private static bool Roslyn_SetupHotReload()
    {
        if (!MetadataUpdater.IsSupported)
        {
            Engine.Log.ONE_Error(nameof(EditorReload), "Hot reload not supported, set DOTNET_MODIFIABLE_ASSEMBLIES=debug");
            return false;
        }

        try
        {
            Assembly? assembly = Assembly.GetEntryAssembly();
            if (assembly == null) return false;
            _assembly = assembly;

            if (!Directory.Exists(EditorCodeGen.OriginalSourceFolder))
            {
                Engine.Log.ONE_Error(nameof(EditorReload), $"The '{EditorCodeGen.OriginalSourceFolder}' folder is missing, do you have the correct settings in your csproj?");
                return false;
            }

            string? projectRoot = EditorCodeGen.ProjectRoot;
            if (projectRoot == null)
            {
                Engine.Log.ONE_Error(nameof(EditorReload), $"The project root is missing.");
                return false;
            }

            Engine.Log.ONE_Info(nameof(EditorReload), "Creating project baseline from the compiled source...");
            Stopwatch sw = Stopwatch.StartNew();

            // Capture all files so we can track modifications
            var allFileSyntaxTrees = new List<SyntaxTree>();
            foreach (string filePath in Directory.EnumerateFiles(EditorCodeGen.OriginalSourceFolder, "*.cs", SearchOption.AllDirectories))
            {
                using FileStream newSourceStream = File.OpenRead(filePath);
                SourceText source = SourceText.From(newSourceStream, System.Text.Encoding.UTF8);

                string relPath = Path.GetRelativePath(EditorCodeGen.OriginalSourceFolder, filePath);
                string originalSourcePath = Path.Join(projectRoot, relPath);
                if (!File.Exists(originalSourcePath)) continue;

                allFileSyntaxTrees.Add(CSharpSyntaxTree.ParseText(source, _parseOptions, originalSourcePath));
            }

#pragma warning disable IL3000 // Avoid accessing Assembly file path when publishing as a single file
            List<MetadataReference> references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(static a => !a.IsDynamic && a.Location != null && File.Exists(a.Location))
                .Select(static a => MetadataReference.CreateFromFile(a.Location!))
                .Cast<MetadataReference>()
                .ToList();

            _compilation = CSharpCompilation.Create(
               assembly.GetName().Name,
               allFileSyntaxTrees,
               references,
               new CSharpCompilationOptions(
                   OutputKind.ConsoleApplication,
                   optimizationLevel: OptimizationLevel.Debug,
                   nullableContextOptions: NullableContextOptions.Enable)
            );

            byte[] dllBytes = File.ReadAllBytes(assembly.Location);
            var moduleMetadata = ModuleMetadata.CreateFromImage(dllBytes);
            _baseline = EmitBaseline.CreateInitialBaseline(
                _compilation,
                moduleMetadata,
                _ => default,
                _ => default,
                false
            );
#pragma warning restore IL3000 // Avoid accessing Assembly file path when publishing as a single file

            Engine.Log.ONE_Info(nameof(EditorReload), $"Baseline creation complete in {sw.ElapsedMilliseconds}ms!");

            Roslyn_ReloadModifiedFiles();
        }
        catch (Exception ex)
        {
            Engine.Log.ONE_Error(nameof(EditorReload), "Failed to setup Roslyn hot reload");
            return false;
        }
        return true;
    }

    private static void Roslyn_ReloadModifiedFiles()
    {
        if (!Directory.Exists(EditorCodeGen.ModifiedSourceFolder)) return;

        foreach (string filePath in Directory.EnumerateFiles(EditorCodeGen.ModifiedSourceFolder, "*.cs", SearchOption.AllDirectories))
        {
            string relPath = Path.GetRelativePath(EditorCodeGen.ModifiedSourceFolder, filePath);

            if (!_modifiedSourceLast.TryGetValue(relPath, out DateTime snapShot) || File.GetLastWriteTimeUtc(filePath) != snapShot)
            {
                Roslyn_ReloadFile(relPath);
            }
        }
    }

    private static void Roslyn_ReloadFile(string filePath)
    {
        string? projectRoot = EditorCodeGen.ProjectRoot;
        AssertNotNull(projectRoot);

        string relPath = Path.GetRelativePath(EditorCodeGen.OriginalSourceFolder, filePath);
        string originalSourcePath = Path.Join(projectRoot, filePath);

        string modifiedSourcePath = Path.Join(EditorCodeGen.ModifiedSourceFolder, filePath);

        using FileStream newSourceStream = File.OpenRead(modifiedSourcePath);
        SourceText source = SourceText.From(newSourceStream, System.Text.Encoding.UTF8);
        SyntaxTree newTree = CSharpSyntaxTree.ParseText(source, _parseOptions, originalSourcePath);
        SyntaxTree? oldTree = _compilation.SyntaxTrees.FirstOrDefault(t => string.Equals(t.FilePath, originalSourcePath, StringComparison.OrdinalIgnoreCase));

        CSharpCompilation newCompilation;
        ImmutableArray<SemanticEdit> edits;

        // New file added (todo: removing files?)
        if (oldTree == null)
        {
            newCompilation = _compilation.AddSyntaxTrees(newTree);
            edits = BuildInsertsFromTree(newCompilation, newTree);
        }
        // File edited
        else
        {
            newCompilation = _compilation.ReplaceSyntaxTree(oldTree, newTree);
            edits = BuildEdits(_compilation, oldTree, newCompilation, newTree);
        }

        using var metadataStream = new MemoryStream();
        using var ilStream = new MemoryStream();
        using var pdbStream = new MemoryStream();

        HashSet<ISymbol?> insertedSymbols = edits
            .Where(e => e.Kind == SemanticEditKind.Insert)
            .Select(e => e.NewSymbol)
            .ToHashSet(SymbolEqualityComparer.Default);

        EmitDifferenceResult result = newCompilation.EmitDifference(
            _baseline,
            edits,
            sym => insertedSymbols.Contains(sym),
            metadataStream,
            ilStream,
            pdbStream,
            CancellationToken.None
        );

        if (!result.Success)
        {
            Engine.Log.ONE_Error(nameof(EditorReload), $"Compile error in {Path.GetFileName(filePath)}:");
            foreach (Diagnostic? d in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                Engine.Log.ONE_Error(nameof(EditorReload), $"  {d}");

            return;
        }

        MetadataUpdater.ApplyUpdate(
            _assembly,
            new ReadOnlySpan<byte>(metadataStream.ToArray()),
            new ReadOnlySpan<byte>(ilStream.ToArray()),
            new ReadOnlySpan<byte>(pdbStream.ToArray())
        );

        _baseline = result.Baseline!;
        _compilation = newCompilation;

        _modifiedSourceLast[filePath] = File.GetLastWriteTimeUtc(filePath);

        Engine.Log.ONE_Info(nameof(EditorReload), $"Reloaded {Path.GetFileName(filePath)}");
    }

    private static ImmutableArray<SemanticEdit> BuildEdits(CSharpCompilation oldComp, SyntaxTree oldTree, CSharpCompilation newComp, SyntaxTree newTree)
    {
        ImmutableArray<SemanticEdit>.Builder edits = ImmutableArray.CreateBuilder<SemanticEdit>();
        SemanticModel oldModel = oldComp.GetSemanticModel(oldTree);
        SemanticModel newModel = newComp.GetSemanticModel(newTree);

        SyntaxNode oldRoot = oldTree.GetRoot();
        SyntaxNode newRoot = newTree.GetRoot();

        Dictionary<string, IMethodSymbol> oldMethods = oldRoot.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Select(m => oldModel.GetDeclaredSymbol(m))
            .OfType<IMethodSymbol>()
            .ToDictionary(s => s.ToDisplayString());

        foreach (MethodDeclarationSyntax newMethodNode in newRoot.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            IMethodSymbol? newSym = newModel.GetDeclaredSymbol(newMethodNode);

            if (newSym == null) continue;
            string key = newSym.ToDisplayString();

            if (oldMethods.TryGetValue(key, out var oldSym))
                edits.Add(new SemanticEdit(SemanticEditKind.Update, oldSym, newSym));
            else
                edits.Add(new SemanticEdit(SemanticEditKind.Insert, null, newSym));
        }

        HashSet<string?> oldTypeNames = oldRoot.DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Select(t => oldModel.GetDeclaredSymbol(t)?.ToDisplayString())
            .Where(s => s != null)
            .ToHashSet();

        foreach (TypeDeclarationSyntax newTypeNode in newRoot.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            INamedTypeSymbol? newSym = newModel.GetDeclaredSymbol(newTypeNode);
            if (newSym != null && !oldTypeNames.Contains(newSym.ToDisplayString()))
                edits.Add(new SemanticEdit(SemanticEditKind.Insert, null, newSym));
        }

        return edits.ToImmutable();
    }

    private static ImmutableArray<SemanticEdit> BuildInsertsFromTree(CSharpCompilation comp, SyntaxTree tree)
    {
        SemanticModel model = comp.GetSemanticModel(tree);
        SyntaxNode root = tree.GetRoot();
        ImmutableArray<SemanticEdit>.Builder edits = ImmutableArray.CreateBuilder<SemanticEdit>();

        foreach (TypeDeclarationSyntax typeNode in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            INamedTypeSymbol? sym = model.GetDeclaredSymbol(typeNode);
            if (sym != null)
                edits.Add(new SemanticEdit(SemanticEditKind.Insert, null, sym));
        }
        return edits.ToImmutable();
    }

    #endregion
}

#endif