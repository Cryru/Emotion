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
	protected HashSet<GameDataObject> _unsaved = new();

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

			var deleteCurrent = new EditorButton
			{
				Id = "DeleteButton",
				StretchY = true,
				Text = "Delete",
				OnClickedProxy = _ =>
				{
					AssertNotNull(_selectedObject);
					GameDataDatabase.EditorDeleteObject(_type, _selectedObject);
					_selectedObject = null;
					RegenerateList();
					RegenerateSelection();
				},
				Enabled = false
            };
            buttonList.AddChild(deleteCurrent);

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
			LayoutMode = LayoutMode.VerticalList,
			MaxSizeX = 200,
			MinSizeX = 200
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

	private void SaveToFile(bool force = false)
	{
        Dictionary<string, GameDataObject>? data = GameDataDatabase.GetObjectsOfType(_type);
		if (data == null) return;
        foreach (KeyValuePair<string, GameDataObject> item in data)
        {
			if (!force && !_unsaved.Contains(item.Value)) continue;

            XMLAsset<GameDataObject> asset = XMLAsset<GameDataObject>.CreateFromContent(item.Value);
            asset.SaveAs(item.Value.AssetPath ?? GameDataDatabase.GetAssetPath(item.Value));

        }

		_unsaved.Clear();
		RegenerateList();
    }

	private void RegenerateList()
	{
		_list.ClearChildren();

		Dictionary<string, GameDataObject>? data = GameDataDatabase.GetObjectsOfType(_type);
		if (data == null) return;

		// Sort the items
		string[] names = new string[data.Count];
		int nameIdx = 0;
        foreach (KeyValuePair<string, GameDataObject> item in data)
        {
			names[nameIdx] = item.Key;
			nameIdx++;
        }
		Array.Sort(names);

		// Create buttons for each of them.
		for (int i = 0; i < names.Length; i++)
		{
			var key = names[i];
			var item = data[key];

            string label = key;
            if (_unsaved.Contains(item)) label += "(*)";

            var uiForItem = new EditorButton
            {
                StretchY = true,
                Text = label,
                UserData = item,
            };
            _list.AddChild(uiForItem);
        }

		_list.SetupMouseSelection();
	}

	private void RegenerateSelection()
	{
		var deleteButton = GetWindowById("DeleteButton");
		if (deleteButton != null)
		{
			var deleteButtAsObject = (EditorButton)deleteButton;
			deleteButtAsObject.Enabled = _selectedObject != null;
		}

		_rightSide.ClearChildren();
		if (_selectedObject == null) return;
		var properties = new GenericPropertiesEditorPanel(_selectedObject)
		{
			PanelMode = PanelMode.Embedded,
			OnPropertyEdited = (propertyName, oldValue) =>
			{
				if (_unsaved.Add(_selectedObject))
				{
					RegenerateList();
				}

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

					// Remove the old file. (the object is resaved with the new id)
					// Keep in mind that if it was already loaded (by the game or such)
					// as an asset it will stay loaded.
					DebugAssetStore.DeleteFile(oldAssetPath);

					GameDataDatabase.EditorReIndex(_type);
					RegenerateList();
				}
			}
		};

		_rightSide.AddChild(properties);
	}
}