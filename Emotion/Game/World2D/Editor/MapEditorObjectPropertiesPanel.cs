#region Using

using System.Diagnostics;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Platform.Input;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
	public sealed class MapEditorObjectPropertiesPanel : MapEditorPanel
	{
		public GameObject2D Object;
		public int ObjectUId;
		public Map2D ObjectMap;

		private Action<GameObject2D> _objectChangedCallback;
		private List<EditorUtility.TypeAndFieldHandlers> _fields;
		private UIBaseWindow? _openEditor;
		private XMLFieldHandler? _openEditorHandler;
		private Action? _editConfirmCallback;

		private bool _objectReferenceInvalidated;

		public MapEditorObjectPropertiesPanel(GameObject2D obj, Action<GameObject2D> objectChangeCallback) : base("Properties")
		{
			Object = obj;
			ObjectUId = Object.UniqueId;
			ObjectMap = Object.Map;

			_objectChangedCallback = objectChangeCallback;
			_fields = EditorUtility.GetTypeFields(obj);
		}

		public override void AttachedToController(UIController controller)
		{
			Header = $"{Object.ObjectName ?? "Object"} [{ObjectUId}] Properties";

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

			var statusLabel = new UIText();
			statusLabel.ScaleMode = UIScaleMode.FloatScale;
			statusLabel.WindowColor = MapEditorColorPalette.TextColor;
			statusLabel.FontFile = "Editor/UbuntuMono-Regular.ttf";
			statusLabel.FontSize = MapEditorColorPalette.EditorButtonTextSize;

			var statusText = Object.ObjectState.ToString();
			ObjectFlags[]? objectFlags = Enum.GetValues<ObjectFlags>();
			for (var i = 0; i < objectFlags.Length; i++)
			{
				ObjectFlags flag = objectFlags[i];
				if (Object.ObjectFlags.HasFlag(flag)) statusText += $", {flag}";
			}
			statusLabel.Text = statusText;

			innerContainer.AddChild(statusLabel);

			var listContainer = new UIBaseWindow();
			listContainer.StretchX = true;
			listContainer.StretchY = true;
			listContainer.InputTransparent = false;
			innerContainer.AddChild(listContainer);

			var listNav = new UICallbackListNavigator();
			listNav.LayoutMode = LayoutMode.VerticalList;
			listNav.StretchX = true;
			listNav.ListSpacing = new Vector2(0, 1);
			listNav.Margins = new Rectangle(0, 0, 10, 0);
			listNav.InputTransparent = false;
			listNav.ChildrenAllSameWidth = true;
			listContainer.AddChild(listNav);

			var scrollBar = new UIScrollbar();
			scrollBar.DefaultSelectorColor = MapEditorColorPalette.ButtonColor;
			scrollBar.SelectorMouseInColor = MapEditorColorPalette.ActiveButtonColor;
			scrollBar.WindowColor = Color.Black * 0.5f;
			scrollBar.Anchor = UIAnchor.TopRight;
			scrollBar.ParentAnchor = UIAnchor.TopRight;
			scrollBar.MinSize = new Vector2(5, 0);
			scrollBar.MaxSize = new Vector2(5, 9999);
			listNav.SetScrollbar(scrollBar);
			listContainer.AddChild(scrollBar);

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
				fieldGroupHeaderContainer.AddChild(fieldGroupHeader);

				listNav.AddChild(fieldGroupHeaderContainer);

				for (var j = 0; j < fieldGroup.Fields.Count; j++)
				{
					XMLFieldHandler field = fieldGroup.Fields[j];

					// todo: map editor readonly property
					// todo: map editor hidden property (like uniqueId)
					if (field.Name == "UniqueId") continue;

					var fieldEditorContainer = new UIBaseWindow();
					fieldEditorContainer.InputTransparent = false;
					fieldEditorContainer.StretchX = true;
					fieldEditorContainer.StretchY = true;
					fieldEditorContainer.Margins = new Rectangle(3, 0, 0, 0);
					fieldEditorContainer.LayoutMode = LayoutMode.HorizontalList;
					fieldEditorContainer.ListSpacing = new Vector2(5, 0);

					var label = new UIText();
					label.ScaleMode = UIScaleMode.FloatScale;
					label.WindowColor = MapEditorColorPalette.TextColor;
					label.FontFile = "Editor/UbuntuMono-Regular.ttf";
					label.FontSize = MapEditorColorPalette.EditorButtonTextSize;
					label.IgnoreParentColor = true;
					label.Anchor = UIAnchor.CenterLeft;
					label.ParentAnchor = UIAnchor.CenterLeft;
					label.Text = field.Name + ": ";
					label.MinSize = new Vector2(50, 0);
					fieldEditorContainer.AddChild(label);

					SpawnEditorButton(field, fieldEditorContainer);

					listNav.AddChild(fieldEditorContainer);
				}
			}
		}

		protected override bool UpdateInternal()
		{
			UIBaseWindow? focus = Controller?.InputFocus;
			if (_openEditor != null && focus != null && !focus.IsWithin(this)) FieldExitEditor();

			if (_objectReferenceInvalidated)
			{
				ObjectReferenceUpdated();
				_objectReferenceInvalidated = false;
				return false;
			}

			return base.UpdateInternal();
		}

		public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
		{
			if (key == Key.Enter && status == KeyStatus.Down && _openEditor != null) _editConfirmCallback?.Invoke();
			if (key == Key.Escape && status == KeyStatus.Down && _openEditor != null) FieldExitEditor();

			return base.OnKey(key, status, mousePos);
		}

		private void FieldEnterEditor(XMLFieldHandler field, UIBaseWindow fieldEditor)
		{
			if (_openEditor != null) FieldExitEditor();

			UIBaseWindow? button = fieldEditor.GetWindowById("EditorButton");
			Debug.Assert(button != null);
			if (button != null) fieldEditor.RemoveChild(button);

			// todo: Insert switch for different types based on field

			UISolidColor editorBg = new UISolidColor();
			editorBg.StretchX = true;
			editorBg.StretchY = true;
			editorBg.WindowColor = Color.Black * 0.7f;
			editorBg.Id = "EditorFieldEditor";
			editorBg.InputTransparent = false;
			fieldEditor.AddChild(editorBg);

			object? propertyValue = field.ReflectionInfo.GetValue(Object);
			var defaultTextValue = (propertyValue ?? "null").ToString()!;
			string editorValue = defaultTextValue;
			if (propertyValue == null) editorValue = "";

			var invisibleTextStretch = new UIText();
			invisibleTextStretch.WindowColor = new Color(230, 230, 230, 100);
			invisibleTextStretch.Text = defaultTextValue;
			invisibleTextStretch.FontFile = "Editor/UbuntuMono-Regular.ttf";
			invisibleTextStretch.FontSize = MapEditorColorPalette.EditorButtonTextSize;
			invisibleTextStretch.Margins = new Rectangle(2, 1, 2, 1);
			invisibleTextStretch.IgnoreParentColor = true;
			editorBg.AddChild(invisibleTextStretch);

			var textInput = new UITextInput();
			textInput.Text = editorValue;
			textInput.WindowColor = MapEditorColorPalette.TextColor;
			textInput.FontFile = "Editor/UbuntuMono-Regular.ttf";
			textInput.FontSize = MapEditorColorPalette.EditorButtonTextSize;
			textInput.SizeOfText = true;
			textInput.Margins = new Rectangle(2, 1, 2, 1);
			textInput.IgnoreParentColor = true;

			_editConfirmCallback = () =>
			{
				string? text = textInput.Text;
				if (field.TypeHandler.Type == typeof(string)) ApplyObjectChange(field, text);
			};

			editorBg.AddChild(textInput);
			Controller?.SetInputFocus(textInput);

			_openEditor = fieldEditor;
			_openEditorHandler = field;
		}

		private void FieldExitEditor()
		{
			if (_openEditorHandler == null || _openEditor == null) return;

			UIBaseWindow? button = _openEditor.GetWindowById("EditorFieldEditor");
			Debug.Assert(button != null);
			if (button != null) _openEditor.RemoveChild(button);

			SpawnEditorButton(_openEditorHandler, _openEditor);

			_openEditor = null;
			_openEditorHandler = null;
			_editConfirmCallback = null;

			Controller?.SetInputFocus(this);
		}

		private void SpawnEditorButton(XMLFieldHandler field, UIBaseWindow fieldEditor)
		{
			var editorButton = new MapEditorTopBarButton();
			editorButton.Text = (field.ReflectionInfo.GetValue(Object) ?? "null").ToString();
			editorButton.StretchY = true;
			editorButton.OnClickedProxy = _ => { FieldEnterEditor(field, fieldEditor); };
			editorButton.Id = "EditorButton";
			fieldEditor.AddChild(editorButton);
		}

		public void ApplyObjectChange(XMLFieldHandler field, object value)
		{
			GameObject2D oldObject = Object;
			Map2D objectMap = oldObject.Map;

			Debug.Assert(objectMap != null);

			// Save the id.
			int id = oldObject.UniqueId;
			oldObject.PreMapEditorSave();
			oldObject.UniqueId = id;
			_objectChangedCallback(oldObject);
			objectMap.RemoveObject(Object, true);

			// Keep parity with UndoAction
			string? objectAsData = XMLFormat.To(oldObject);
			var newObject = XMLFormat.From<GameObject2D>(objectAsData);

			field.ReflectionInfo.SetValue(newObject, value);
			objectMap.AddObject(newObject);
		}

		public void InvalidateObjectReference()
		{
			_objectReferenceInvalidated = true;
		}

		private void ObjectReferenceUpdated()
		{
			FieldExitEditor();

			// Try to find the object we were showing.
			GameObject2D? newObjectReference = null;
			foreach (GameObject2D obj in ObjectMap.GetObjects())
			{
				if (obj.UniqueId == ObjectUId)
				{
					newObjectReference = obj;
					break;
				}
			}

			if (newObjectReference == null)
			{
				Controller?.RemoveChild(this);
				return;
			}

			Debug.Assert(newObjectReference.GetType() == Object.GetType());
			Object = newObjectReference;

			UIBaseWindow? parent = Parent;
			parent!.RemoveChild(this);
			ClearChildren(); // Clear old ui.
			parent.AddChild(this); // Reattaching will cause the new ui to spawn.
		}
	}
}