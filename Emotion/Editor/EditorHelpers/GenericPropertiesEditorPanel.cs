#region Using

using Emotion.Editor.PropertyEditors;
using Emotion.Game.World.Editor;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.UI;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Editor.EditorHelpers;

public class GenericPropertiesEditorPanel : EditorPanel
{
	public Action<string, object?>? OnPropertyEdited;

	protected List<EditorUtility.TypeAndFieldHandlers> _fields;
	protected List<IPropEditorGeneric> _editorUIs;

	protected object _obj;

	protected bool _spawnFieldGroupHeaders = true;

	public GenericPropertiesEditorPanel(object obj) : base($"{obj} Properties")
	{
		_obj = obj;
		_fields = EditorUtility.GetTypeFields(obj);
		_editorUIs = new();
	}

	public override void AttachedToController(UIController controller)
	{
		base.AttachedToController(controller);

		var innerContainer = new UIBaseWindow();
		innerContainer.StretchX = true;
		innerContainer.StretchY = true;
		innerContainer.LayoutMode = LayoutMode.VerticalList;
		innerContainer.ListSpacing = new Vector2(0, 3);
		innerContainer.ChildrenAllSameWidth = true;
		innerContainer.Id = "InnerContainer";
		_contentParent.AddChild(innerContainer);

		var listContainer = new UIBaseWindow();
		listContainer.StretchX = true;
		listContainer.StretchY = true;
		listContainer.LayoutMode = LayoutMode.HorizontalList;
		listContainer.ZOffset = 10;
		innerContainer.AddChild(listContainer);

		var listNav = new UICallbackListNavigator();
		listNav.LayoutMode = LayoutMode.VerticalList;
		listNav.StretchX = true;
		listNav.ListSpacing = new Vector2(0, 1);
		listNav.Margins = new Rectangle(0, 0, 5, 0);
		listNav.ChildrenAllSameWidth = true;
		listNav.MinSizeX = 100;
		listContainer.AddChild(listNav);

		var scrollBar = new EditorScrollBar();
		listNav.SetScrollbar(scrollBar);
		listContainer.AddChild(scrollBar);

		// For each group of fields (inherited from the same class)
		for (var i = 0; i < _fields.Count; i++)
		{
			EditorUtility.TypeAndFieldHandlers fieldGroup = _fields[i];

			var fieldGroupHeaderContainer = new UIBaseWindow();
			fieldGroupHeaderContainer.StretchX = true;
			fieldGroupHeaderContainer.StretchY = true;

			if (_spawnFieldGroupHeaders)
			{
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
			}

			listNav.AddChild(fieldGroupHeaderContainer);

			// For each field
			for (var j = 0; j < fieldGroup.Fields.Count; j++)
			{
				XMLFieldHandler field = fieldGroup.Fields[j];
				IPropEditorGeneric? editor = AddEditorForField(field);
				if (editor != null)
				{
					editor.Field = field;

					object? propertyValue = field.ReflectionInfo.GetValue(_obj);
					editor.SetValue(propertyValue); // Initialize value before attaching callback.
					editor.SetCallbackValueChanged(newValue => { ApplyObjectChange(editor, field, newValue); });

					var editorAsWnd = (UIBaseWindow) editor;
					editorAsWnd.Anchor = UIAnchor.CenterRight;
					editorAsWnd.ParentAnchor = UIAnchor.CenterRight;
					editorAsWnd.ZOffset = 10;

					_editorUIs.Add(editor);
				}

				var editorWithLabel = new FieldEditorWithLabel($"{field.Name}: ", editor);
				OnFieldEditorCreated(field, editor, editorWithLabel);
				listNav.AddChild(editorWithLabel);
			}
		}

		//UpdatePropertyValues();
	}

	protected virtual void OnFieldEditorCreated(XMLFieldHandler field, IPropEditorGeneric? editor, FieldEditorWithLabel editorWithLabel)
	{
	}

	protected virtual void OnFieldEditorUpdated(XMLFieldHandler field, IPropEditorGeneric? editor, FieldEditorWithLabel editorWithLabel)
	{
	}

	private IPropEditorGeneric? AddEditorForField(XMLFieldHandler field)
	{
		if (field.TypeHandler.Type == typeof(Vector2)) return new PropEditorFloat2();
		if (field.TypeHandler.Type == typeof(float)) return new PropEditorNumber<float>();
		if (field.TypeHandler.Type == typeof(int)) return new PropEditorNumber<int>();
		if (field.TypeHandler.Type == typeof(Vector3)) return new PropEditorFloat3();
		if (field.TypeHandler.Type == typeof(Rectangle)) return new PropEditorRect();
		if (field.TypeHandler.Type == typeof(string))
		{
			var assetFileNameAttribute = field.ReflectionInfo.GetAttribute<AssetFileNameAttribute>();
			if (assetFileNameAttribute != null)
				return new PropEditorStringPath(assetFileNameAttribute);

			return new PropEditorString();
		}

		if (field.TypeHandler.Type == typeof(bool)) return new PropEditorBool();
		if (field.TypeHandler.Type.IsEnum) return new PropEditorEnum(field.TypeHandler.Type, field.ReflectionInfo.Nullable);

		return new PropEditorNone();
	}

	protected virtual void ApplyObjectChange(IPropEditorGeneric editor, XMLFieldHandler field, object value)
	{
		object? oldValue = field.ReflectionInfo.GetValue(_obj);
		field.ReflectionInfo.SetValue(_obj, value);

		if (!Helpers.AreObjectsEqual(oldValue, value)) OnPropertyEdited?.Invoke(field.Name, oldValue);

		UIBaseWindow? editorWindow = editor as UIBaseWindow;
		OnFieldEditorUpdated(field, editor, (FieldEditorWithLabel) editorWindow?.Parent!);
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
			IPropEditorGeneric editor = _editorUIs[i];
			UIBaseWindow? editorWindow = editor as UIBaseWindow;

			if (Controller?.InputFocus != null && editorWindow != null && Controller.InputFocus.IsWithin(editorWindow)) continue;

			XMLFieldHandler? field = editor.Field;
			object? propertyValue = field.ReflectionInfo.GetValue(_obj);
			if (!Helpers.AreObjectsEqual(editor.GetValue(), propertyValue))
			{
				editor.SetValue(propertyValue);
				OnFieldEditorUpdated(field, editor, (FieldEditorWithLabel) editorWindow?.Parent!);
			}
		}
	}
}