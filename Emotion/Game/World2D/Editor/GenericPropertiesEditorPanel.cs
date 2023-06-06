#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Game.World2D.Editor;

public class GenericPropertiesEditorPanel : MapEditorPanel
{
	protected List<EditorUtility.TypeAndFieldHandlers> _fields;
	protected List<IMapEditorGeneric> _editorUIs;

	protected object _obj;

	public GenericPropertiesEditorPanel(object obj) : base($"{obj} Properties")
	{
		_obj = obj;
		_fields = EditorUtility.GetTypeFields(obj);
		_editorUIs = new();
	}

	protected virtual void AddHeaderUI(UIBaseWindow uiContainer)
	{
	}

	public override void AttachedToController(UIController controller)
	{
		base.AttachedToController(controller);

		UIBaseWindow contentWin = _contentParent;
		contentWin.InputTransparent = false;

		var innerContainer = new UIBaseWindow();
		innerContainer.StretchX = true;
		innerContainer.StretchY = true;
		innerContainer.InputTransparent = false;
		innerContainer.LayoutMode = LayoutMode.VerticalList;
		innerContainer.ListSpacing = new Vector2(0, 3);
		innerContainer.ChildrenAllSameWidth = true;
		contentWin.AddChild(innerContainer);

		AddHeaderUI(innerContainer);

		var listContainer = new UIBaseWindow();
		listContainer.StretchX = true;
		listContainer.StretchY = true;
		listContainer.InputTransparent = false;
		listContainer.LayoutMode = LayoutMode.HorizontalList;
		innerContainer.AddChild(listContainer);

		var listNav = new UICallbackListNavigator();
		listNav.LayoutMode = LayoutMode.VerticalList;
		listNav.StretchX = true;
		listNav.ListSpacing = new Vector2(0, 1);
		listNav.Margins = new Rectangle(0, 0, 5, 0);
		listNav.InputTransparent = false;
		listNav.ChildrenAllSameWidth = true;
		listContainer.AddChild(listNav);

		var scrollBar = new EditorScrollBar();
		listNav.SetScrollbar(scrollBar);
		listContainer.AddChild(scrollBar);

		// For each group of fields (inherited from the same class)
		for (var i = 0; i < _fields.Count; i++)
		{
			EditorUtility.TypeAndFieldHandlers fieldGroup = _fields[i];

			var fieldGroupHeaderContainer = new UIBaseWindow();
			fieldGroupHeaderContainer.InputTransparent = false;
			fieldGroupHeaderContainer.StretchX = true;
			fieldGroupHeaderContainer.StretchY = true;

			var fieldGroupHeader = new UIText();
			fieldGroupHeader.ScaleMode = UIScaleMode.FloatScale;
			fieldGroupHeader.WindowColor = MapEditorColorPalette.TextColor;
			fieldGroupHeader.FontFile = "Editor/UbuntuMono-Regular.ttf";
			fieldGroupHeader.FontSize = MapEditorColorPalette.EditorButtonTextSize + 1;
			fieldGroupHeader.Underline = true;
			fieldGroupHeader.IgnoreParentColor = true;
			fieldGroupHeader.Text = fieldGroup.DeclaringType.Name;
			fieldGroupHeader.MinSize = new Vector2(0, 11);
			fieldGroupHeaderContainer.AddChild(fieldGroupHeader);

			listNav.AddChild(fieldGroupHeaderContainer);

			// For each field
			for (var j = 0; j < fieldGroup.Fields.Count; j++)
			{
				XMLFieldHandler field = fieldGroup.Fields[j];
				IMapEditorGeneric? editor = AddEditorForField(field);
				if (editor != null)
				{
					editor.Field = field;
					editor.SetCallbackValueChanged(newValue => { ApplyObjectChange(field, newValue); });

					var editorAsWnd = (UIBaseWindow) editor;
					editorAsWnd.Anchor = UIAnchor.CenterRight;
					editorAsWnd.ParentAnchor = UIAnchor.CenterRight;

					_editorUIs.Add(editor);
				}

				listNav.AddChild(new FieldEditorWithLabel($"{field.Name}: ", editor));
			}
		}

		UpdatePropertyValues();
	}

	private IMapEditorGeneric? AddEditorForField(XMLFieldHandler field)
	{
		if (field.TypeHandler.Type == typeof(Vector2)) return new MapEditorFloat2();
		if (field.TypeHandler.Type == typeof(float)) return new MapEditorNumber<float>();
		if (field.TypeHandler.Type == typeof(int)) return new MapEditorNumber<int>();
		if (field.TypeHandler.Type == typeof(Vector3)) return new MapEditorFloat3();
		if (field.TypeHandler.Type == typeof(string)) return new MapEditorString();
		if (field.TypeHandler.Type == typeof(bool)) return new MapEditorBool();
		if (field.TypeHandler.Type.IsEnum) return new MapEditorEnum(field.TypeHandler.Type);

		return null;
	}

	protected virtual void ApplyObjectChange(XMLFieldHandler field, object value)
	{
		field.ReflectionInfo.SetValue(_obj, value);
	}

	protected override bool UpdateInternal()
	{
		UpdatePropertyValues();
		return base.UpdateInternal();
	}

	protected virtual void UpdatePropertyValues()
	{
		for (var i = 0; i < _editorUIs.Count; i++)
		{
			IMapEditorGeneric editor = _editorUIs[i];

			if (Controller?.InputFocus != null && editor is UIBaseWindow editorWindow && Controller.InputFocus.IsWithin(editorWindow)) continue;

			XMLFieldHandler? field = editor.Field;
			object? propertyValue = field.ReflectionInfo.GetValue(_obj);
			if (editor.GetValue() != propertyValue) editor.SetValue(propertyValue);
		}
	}
}