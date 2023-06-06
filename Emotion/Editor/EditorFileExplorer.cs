#region Using

using System.Linq;
using System.Threading.Tasks;
using Emotion.Editor.EditorHelpers;
using Emotion.Game;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.IO;
using Emotion.UI;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Editor;

// todo: add asset -> open host dialog that allows filesystem access then copy to debug store and load in.
// todo: Check if folders contain valid files?
// todo: display folder file count
// todo: allow typing in path
// todo: hide debug store
public class EditorFileExplorer<T> : MapEditorPanel where T : Asset, new()
{
	public bool UseAssetLoaderCache;

	private Action<T> _onFileSelected;
	private Func<string, bool>? _fileFilter;
	private Tree<string, string> _fileSystem;

	private Task? _loadingTask;

	private string[]? _currentBranch;

	public EditorFileExplorer(Action<T> onFileSelected, Func<string, bool>? fileFilter = null) : base($"Select [{typeof(T).Name}]")
	{
		_onFileSelected = onFileSelected;
		_fileFilter = fileFilter;
		UseAssetLoaderCache = false;
		_fileSystem = FilesToTree(Engine.AssetLoader.AllAssets);
	}

	public static Tree<string, string> FilesToTree(IEnumerable<string> assets)
	{
		assets = assets.OrderBy(x => x);
		var tree = new Tree<string, string>();
		foreach (string a in assets)
		{
			if (a.Contains('/'))
			{
				string[] folderPath = a.Split('/')[..^1];
				tree.Add(folderPath, a);
			}
			else
			{
				tree.Leaves.Add(a);
			}
		}

		return tree;
	}

	private void SetTreeBranch(string[]? branch)
	{
		_currentBranch = branch;
		GenerateUIForCurrentBranch();
	}

	private void GenerateUIForCurrentBranch()
	{
		_contentParent.LayoutMode = LayoutMode.VerticalList;
		_contentParent.ClearChildren();

		var currentPath = new MapEditorLabel($"@: Assets{(_currentBranch != null ? $"/{string.Join("/", _currentBranch)}/" : "")}")
		{
			Margins = new Rectangle(0, 0, 0, 5)
		};
		_contentParent.AddChild(currentPath);

		var listContainer = new UIBaseWindow
		{
			StretchX = true,
			StretchY = true,
			InputTransparent = false,
			LayoutMode = LayoutMode.HorizontalList
		};

		Tree<string, string>? branch = _currentBranch == null ? _fileSystem : _fileSystem.GetBranchFromPath(_currentBranch);
		var list = new UICallbackListNavigator
		{
			MinSize = new Vector2(300, 200),
			MaxSize = new Vector2(300, DefaultMaxSizeF),
			InputTransparent = false,
			LayoutMode = LayoutMode.HorizontalListWrap,
			ListSpacing = new Vector2(5, 0),
			Margins = new Rectangle(0, 0, 5, 0),
			Id = "BranchContainer"
		};

		var scrollBar = new EditorScrollBar();
		list.SetScrollbar(scrollBar);
		listContainer.AddChild(list);
		listContainer.AddChild(scrollBar);
		_contentParent.AddChild(listContainer);

		if (branch == null) return;

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

		for (var i = 0; i < branch.Branches.Count; i++)
		{
			Tree<string, string>? subBranch = branch.Branches[i];

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

		for (var i = 0; i < branch.Leaves.Count; i++)
		{
			string? leaf = branch.Leaves[i];

			var button = new FileExplorerButton
			{
				OnClickedProxy = _ => FileSelected(leaf)
			};
			button.SetFileName(leaf);

			// Check if this file is valid.
			if (_fileFilter != null && !_fileFilter(leaf))
			{
				button.WindowColor = button.WindowColor.SetAlpha(50);
				button.OnClickedProxy = null;
			}

			list.AddChild(button);
		}
	}

	public void FileSelected(string fileName)
	{
		if (_loadingTask != null && !_loadingTask.IsCompleted) return;

		_loadingTask = Task.Run(() =>
		{
			T? file = ExplorerLoadAsset(fileName, UseAssetLoaderCache);
			if (file == null) return;
			if (file is XMLAssetMarkerClass xmlFile && !xmlFile.HasContent()) return;

			_onFileSelected.Invoke(file);
			Parent?.RemoveChild(this);
		});
	}

	public T? ExplorerLoadAsset(string name, bool useAssetLoaderCache = false)
	{
		try
		{
			// Load through the asset loader.
			var asset = Engine.AssetLoader.Get<T>(name, useAssetLoaderCache);
			if (asset != null) return asset;
		}
		catch (Exception ex)
		{
			Engine.Log.Warning($"Couldn't load asset - {ex}", "FileExplorerTool");
			throw;
		}

		return null;
	}

	public override void AttachedToController(UIController controller)
	{
		base.AttachedToController(controller);
		SetTreeBranch(null);
	}
}