#nullable enable

#region Using

using System.IO;
using System.Reflection;
using Emotion.Editor.EditorHelpers;
using Emotion.Editor.EditorWindows.DataEditorUtil;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.IO;
using Emotion.UI;

#endregion

namespace Emotion.Editor.EditorWindows;

public class DataEditorGeneric : EditorPanel
{
	protected Type _type;
	protected UICallbackListNavigator _list = null!;
	protected UIBaseWindow _rightSide = null!;
	protected GameDataObject? _selectedObject;

	public DataEditorGeneric(Type type) : base($"{type.Name} Editor")
	{
		_type = type;
	}

	public override void AttachedToController(UIController controller)
	{
		base.AttachedToController(controller);

		var leftPart = new UIBaseWindow
		{
			StretchX = true,
			StretchY = true,
			LayoutMode = LayoutMode.VerticalList
		};
		_contentParent.AddChild(leftPart);

		{
			var buttonList = new UIBaseWindow
			{
				StretchX = true,
				StretchY = true,
				LayoutMode = LayoutMode.HorizontalList,
				Margins = new Rectangle(0, 0, 0, 5),
				ListSpacing = new Vector2(5, 0)
			};
			leftPart.AddChild(buttonList);

			var newButton = new EditorButton
			{
				StretchY = true,
				Text = "New",
				OnClickedProxy = _ => { CreateNew(); }
			};
			buttonList.AddChild(newButton);

			var saveButton = new EditorButton
			{
				StretchY = true,
				Text = "Save",
				OnClickedProxy = _ => { SaveToFile(); }
			};
			buttonList.AddChild(saveButton);

			var listContainer = new UIBaseWindow
			{
				StretchX = true,
				StretchY = true,
				LayoutMode = LayoutMode.HorizontalList,
				ZOffset = 10
			};
			leftPart.AddChild(listContainer);

			var listNav = new UICallbackListNavigator
			{
				LayoutMode = LayoutMode.VerticalList,
				StretchX = true,
				ListSpacing = new Vector2(0, 1),
				Margins = new Rectangle(0, 0, 5, 0),
				ChildrenAllSameWidth = true,
				MinSizeX = 100
			};
			listNav.OnChoiceConfirmed += (wnd, idx) =>
			{
				if (wnd is not EditorButton nuSelButton) return;

				object? userData = nuSelButton.UserData;
				if (userData == null || !userData.GetType().IsAssignableTo(_type)) return;

				_selectedObject = (GameDataObject) userData;
				RegenerateSelection();
			};
			listContainer.AddChild(listNav);

			var scrollBar = new EditorScrollBar();
			listNav.SetScrollbar(scrollBar);
			listContainer.AddChild(scrollBar);

			_list = listNav;
		}

		var rightPart = new UIBaseWindow
		{
			StretchX = true,
			StretchY = true,
			LayoutMode = LayoutMode.VerticalList
		};
		_rightSide = rightPart;
		_contentParent.AddChild(rightPart);

		_contentParent.LayoutMode = LayoutMode.HorizontalList;
		RegenerateList();
	}

	private void CreateNew()
	{
		ConstructorInfo constructor = _type.GetConstructor(Type.EmptyTypes)!;
		var newObj = (GameDataObject) constructor.Invoke(null);
		newObj.Id = "Untitled";

		GameDataDatabase.EditorAddObject(_type, newObj);
		RegenerateList();
	}

	private void SaveToFile()
	{
		if (_selectedObject == null || _selectedObject.AssetPath == null) return;
		XMLAsset<GameDataObject> asset = XMLAsset<GameDataObject>.CreateFromContent(_selectedObject, _selectedObject.AssetPath);
		asset.Save();
	}

	private void RegenerateList()
	{
		_list.ClearChildren();

		Dictionary<string, GameDataObject>? data = GameDataDatabase.GetObjectsOfType(_type);
		if (data == null) return;

		foreach (KeyValuePair<string, GameDataObject> item in data)
		{
			var uiForItem = new EditorButton
			{
				StretchY = true,
				Text = item.Key,
				UserData = item.Value
			};
			_list.AddChild(uiForItem);
		}

		_list.SetupMouseSelection();
	}

	private void RegenerateSelection()
	{
		_rightSide.ClearChildren();
		if (_selectedObject == null) return;
		var properties = new GenericPropertiesEditorPanel(_selectedObject)
		{
			PanelMode = PanelMode.Embedded,
			OnPropertyEdited = (propertyName, oldValue) =>
			{
				// If id is changed we need to change the save file as well.
				// todo: think about assigning generated non-user displayed ids to files
				if (propertyName == "Id")
				{
					string newId = GameDataDatabase.EnsureNonDuplicatedId(_selectedObject.Id, _type);
					_selectedObject.Id = newId;

					string newPath = GameDataDatabase.GetAssetPath(_selectedObject);
					string? oldAssetPath = _selectedObject.AssetPath;
					_selectedObject.AssetPath = newPath;
					SaveToFile();

					// Remove the old file.
					// Keep in mind that if it was already loaded (by the game or such)
					// as an asset it will stay loaded.
					string? path = DebugAssetStore.AssetDevPath;
					string oldFileSystemPath = Path.Join(path, oldAssetPath);
					if (File.Exists(oldFileSystemPath)) File.Delete(oldFileSystemPath);
					string assetPath = Path.Join("Assets", oldAssetPath);
					if (File.Exists(assetPath)) File.Delete(assetPath);

					GameDataDatabase.EditorReIndex(_type);
					RegenerateList();
				}
			}
		};

		_rightSide.AddChild(properties);
	}
}