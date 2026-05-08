#nullable enable

#if DEBUG
using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using System.IO;
#endif

namespace Emotion.Editor.RuntimeCodeGen;

public static class EditorCodeGen
{
    public static bool CanRuntimeCodeGen
    {
#if DEBUG
        get => ProjectRoot != null && EditorReload.Type != ReloadType.None;
#else
        get => false;
#endif
    }

#if DEBUG

    public static string ModifiedSourceFolder { get => $".{Path.DirectorySeparatorChar}ModifiedSource"; }
    public static string OriginalSourceFolder { get => $".{Path.DirectorySeparatorChar}CompiledSource"; }

    public static string? ProjectRoot { get; private set; }

    internal static void Init()
    {
        if (!Directory.Exists(ModifiedSourceFolder))
        {
            Engine.Log.ONE_Info(nameof(EditorCodeGen), "Preparing runtime code gen async.");

            // Probably launched through visual studio, we can async reload
            // since there probably aren't any modifications (fresh compilation)
            Engine.Jobs.AddNoFeedback(InitRoutineAsync());
        }
        else
        {
            Engine.Log.ONE_Info(nameof(EditorCodeGen), "Preparing runtime code gen...");

            // If running outside visual studio we want to apply modifications before the game loads,
            // so we block the start.
            Coroutine.RunInline(InitRoutineAsync());
        }
    }

    private static IEnumerator InitRoutineAsync()
    {
        string projectFolderAssetLoader = AssetLoader.DevModeProjectFolder;
        if (string.IsNullOrEmpty(projectFolderAssetLoader))
        {
            Engine.Log.ONE_Error(nameof(EditorCodeGen), $"Couldn't find project root directory, started at {AppContext.BaseDirectory}");
            yield break;
        }
        ProjectRoot = Path.GetFullPath(projectFolderAssetLoader);

        EditorReload.Init();
    }

    public static void SubmitCodeGen(string filePath, string fileContent)
    {
        try
        {
            if (ProjectRoot == null)
            {
                Engine.Log.ONE_Error(nameof(EditorCodeGen), $"Couldn't save {filePath} as no project root was discovered.");
            }
            else
            {
                string projectSource = Path.Join(ProjectRoot, filePath);
                SafelyWrite(projectSource, fileContent);
                Engine.Log.ONE_Info(nameof(EditorCodeGen), $"Wrote to {projectSource}");
            }

            string runtimeSource = Path.Join(ModifiedSourceFolder, filePath);
            SafelyWrite(runtimeSource, fileContent);

            EditorReload.TryReload();
        }
        catch (Exception ex)
        {
            Engine.Log.ONE_Error(nameof(EditorCodeGen), $"Error during code gen - {ex}");
        }
    }

    private static void SafelyWrite(string path, string content)
    {
        string? d = Path.GetDirectoryName(path);
        if (d != null) Directory.CreateDirectory(d);
        File.WriteAllText(path, content);
    }
#else
    internal static void Init()
    {

    }
#endif
}
