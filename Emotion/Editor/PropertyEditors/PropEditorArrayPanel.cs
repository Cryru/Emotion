#nullable enable

#region Using

using System.Reflection;
using Emotion.Editor.EditorHelpers;
using Emotion.Editor.EditorWindows.DataEditorUtil;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.IO;
using Emotion.Standard.TMX;
using Emotion.Standard.XML;
using Emotion.UI;
using Emotion.Utility;

#endregion

namespace Emotion.Editor.PropertyEditors;

public class PropEditorArrayPanel : EditorPanel
{
	protected PropEditorArray _propEditor;
	protected bool _canCreateItems;

	protected UICallbackListNavigator _list = null!;
	protected UIBaseWindow _rightSide = null!;
	protected int _selectedObjectIdx = -1;

	public PropEditorArrayPanel(PropEditorArray propEditor) : base($"Array Editor - Field {propEditor.Field.Name}")
	{
		_propEditor = propEditor;
		var elementType = propEditor.GetElementType();
		_canCreateItems = EditorUtility.HasParameterlessConstructor(elementType);
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
				Text = "Add",
				OnClickedProxy = _ =>
				{ 
					AddNewToArray();
				},
				Enabled = _canCreateItems
			};
			buttonList.AddChild(newButton);

			var deleteCurrent = new EditorButton
			{
				Id = "DeleteButton",
				StretchY = true,
				Text = "Delete",
				OnClickedProxy = _ =>
				{
					RemoveSelectedFromArray();
				},
				Enabled = false
			};
			buttonList.AddChild(deleteCurrent);

			if (!_canCreateItems)
			{
				var editorLabel = new MapEditorLabel("Element type has no parameterless constructor\nand cannot be serialized (or edited).");
				leftPart.AddChild(editorLabel);
			}

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
				int? userData = nuSelButton.UserData as int?;
				_selectedObjectIdx = userData ?? -1;
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

	private void AddNewToArray()
	{
		AssertNotNull(_propEditor.Field);
		_propEditor.CreateItem();
		RegenerateList();
	}

	private void RemoveSelectedFromArray()
	{
		_propEditor.RemoveItemAtIndex(_selectedObjectIdx);
		_selectedObjectIdx = -1;
		RegenerateList();
		RegenerateSelection();
	}

	private void RegenerateList()
	{
		_list.ClearChildren();

		var length = _propEditor.GetLength();
		for (int i = 0; i < length; i++)
		{
			var item = _propEditor.GetItemAtIndex(i);
			var itemType = item?.GetType();
			string itemString = item?.ToString() ?? "<null>";

			var uiForItem = new EditorButton
			{
				StretchY = true,
				Text = $"[{i}] {(itemString == itemType?.ToString() ? XMLHelpers.GetTypeName(itemType) : itemString)}",
				UserData = i,
			};
			uiForItem.MaxSizeX = 200;
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
			deleteButtAsObject.Enabled = _selectedObjectIdx != -1;
		}

		_rightSide.ClearChildren();

		if (_selectedObjectIdx == -1) return;

		var selectedObj = _propEditor.GetItemAtIndex(_selectedObjectIdx);
		if (selectedObj == null) return;
		var properties = new GenericPropertiesEditorPanel(selectedObj)
		{
			PanelMode = PanelMode.Embedded,
			OnNonComplexTypeValueChanged = (value) =>
			{
				_propEditor.SetItemAtIndex(_selectedObjectIdx, value);
				_propEditor.ArrayItemModified(_selectedObjectIdx);
				RegenerateList();
			},
			OnPropertyEdited = (_, __) =>
			{
				_propEditor.ArrayItemModified(_selectedObjectIdx);
				RegenerateList();
			}
		};

		_rightSide.AddChild(properties);
	}
}