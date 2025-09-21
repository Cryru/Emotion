#nullable enable

using Emotion.Core.Platform.Implementation.CommonDesktop;
using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.FilePickerHelpers;
using Emotion.Game.Systems.UI;
using Emotion.Primitives.DataStructures;
using Emotion.Standard.Reflector;

namespace Emotion.Editor.EditorUI;

public class FilePicker<T> : EditorWindow where T : Asset, new()
{
    public bool UseAssetLoaderCache;
    public Func<string, bool>? FileFilterFunc;

    private Action<T> _onFileSelected;
    private NTree<string, string> _fileSystem;

    private Coroutine _loadingRoutine = Coroutine.CompletedRoutine;

    private string[]? _currentBranch;

    public FilePicker(Action<T> onFileSelected, Func<string, bool>? fileFilter = null)
        : base($"Select [{ReflectorEngine.GetTypeName(typeof(T))}]")
    {
        PanelMode = PanelMode.Modal;

        _onFileSelected = onFileSelected;
        FileFilterFunc = fileFilter;
        UseAssetLoaderCache = false;
        _fileSystem = Engine.AssetLoader.GetAssetFileTree();
    }

    private void SetTreeBranch(string[]? branch)
    {
        _currentBranch = branch;
        GenerateUIForCurrentBranch();
    }

    private void GenerateUIForCurrentBranch()
    {
        EditorLabel? currentPathLabel = GetWindowById<EditorLabel>("CurrentPathLabel");
        if (currentPathLabel != null)
            currentPathLabel.Text = $"@: Assets{(_currentBranch != null ? $"/{string.Join("/", _currentBranch)}/" : "")}";

        NTree<string, string>? branch = _currentBranch == null ? _fileSystem : _fileSystem.GetBranchFromPath(_currentBranch);

        UIBaseWindow? list = GetWindowById("ContainerList");
        list?.ClearChildren();
        if (list != null && branch != null)
        {
            if (_currentBranch != null)
            {
                var button = new FileExplorerButton
                {
                    OnClickedProxy = _ =>
                    {
                        _currentBranch = _currentBranch.RemoveFromArray(_currentBranch.Length - 1);
                        if (_currentBranch.Length == 0) _currentBranch = null;
                        SetTreeBranch(_currentBranch);
                    }
                };
                button.SetDirectory("<Back>");
                list.AddChild(button);
            }

            // Put in other branches
            for (var i = 0; i < branch.Branches.Count; i++)
            {
                NTree<string, string>? subBranch = branch.Branches[i];

                var button = new FileExplorerButton
                {
                    OnClickedProxy = _ =>
                    {
                        _currentBranch ??= Array.Empty<string>();
                        _currentBranch = _currentBranch.AddToArray(subBranch.Name);
                        SetTreeBranch(_currentBranch);
                    }
                };
                button.SetDirectory(subBranch.Name);

                list.AddChild(button);
            }

            // Put in filtered first
            for (var i = 0; i < branch.Leaves.Count; i++)
            {
                string? leaf = branch.Leaves[i];

                // Check if this file is valid.
                if (FileFilterFunc != null && FileFilterFunc(leaf))
                {
                    var button = new FileExplorerButton
                    {
                        OnClickedProxy = _ => FileSelected(leaf)
                    };
                    button.SetFileName(leaf);

                    list.AddChild(button);
                }
            }

            // Then all else
            for (var i = 0; i < branch.Leaves.Count; i++)
            {
                string? leaf = branch.Leaves[i];

                // Check if this file is valid.
                if (FileFilterFunc == null || !FileFilterFunc(leaf))
                {
                    var button = new FileExplorerButton
                    {
                        OnClickedProxy = _ => FileSelected(leaf)
                    };
                    button.SetFileName(leaf);

                    if (FileFilterFunc != null)
                    {
                        button.WindowColor = button.WindowColor.SetAlpha(50);
                        button.OnClickedProxy = null;
                    }

                    list.AddChild(button);
                }
            }
        }
    }

    protected override void OnOpen()
    {
        base.OnOpen();

        UIBaseWindow contentParent = GetContentParent();

        UIBaseWindow container = new UIBaseWindow
        {
            Name = "MainContainer",
            LayoutMode = LayoutMode.VerticalList,
            Paddings = new Rectangle(10, 10, 10, 10),
            ListSpacing = new Vector2(0, 10)
        };
        contentParent.AddChild(container);

        EditorLabel label = new EditorLabel("Loading")
        {
            Name = "CurrentPathLabel",
        };
        container.AddChild(label);

        UIBaseWindow containerList = new()
        {
            Name = "ContainerList",
            LayoutMode = LayoutMode.HorizontalListWrap,
            ListSpacing = new Vector2(10, 10),
            GrowX = false,
            GrowY = false,

            MinSizeX = 1250,
            MaxSizeX = 1250,

            MinSizeY = 500,
            MaxSizeY = 500,
        };
        container.AddChild(containerList);

        SetTreeBranch(null);
    }

    public void FileSelected(string fileName)
    {
        if (!_loadingRoutine.Finished) return;
        _loadingRoutine = Engine.CoroutineManager.StartCoroutine(LoadFileRoutine(fileName));
    }

    private IEnumerator LoadFileRoutine(string name)
    {
        T? asset = null;

        // Load through the asset loader.
        var assetLoadTask = Engine.AssetLoader.GetAsync<T>(name, UseAssetLoaderCache);
        yield return new TaskRoutineWaiter(assetLoadTask);
        asset = assetLoadTask.Result;

        // Verify
        if (asset == null) yield break;
        if (asset is XMLAssetMarkerClass xmlFile && !xmlFile.HasContent()) yield break;

        _onFileSelected.Invoke(asset);
        Parent?.RemoveChild(this);
        yield break;
    }

    public static void SelectFile(UIBaseWindow window, Action<T?> onLoaded)
    {
        var platform = Engine.Host;
        if (platform is DesktopPlatform winPl)
            winPl.DeveloperMode_SelectFileNative(FileLoadProxy(onLoaded));
    }

    // We need these proxies to ensure that the asset is loaded before calling the callback
    // and that the callback is called on the GLThread and not during any UI/Editor updates.
    private static Action<T?> FileLoadProxy(Action<T?> userFunction)
    {
        return (file) =>
        {
            if (file == null) return;
            Engine.CoroutineManager.StartCoroutine(FileLoadProxyRoutine(userFunction, file));
        };
    }

    private static IEnumerator FileLoadProxyRoutine(Action<T?> userFunction, T file)
    {
        yield return file; // Wait for loading
        userFunction(file);
    }
}
