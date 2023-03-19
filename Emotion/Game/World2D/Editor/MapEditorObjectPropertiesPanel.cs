#region Using

using System.Diagnostics;
using System.Text;
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

			var metaText = new StringBuilder();
			metaText.Append(Object.ObjectState.ToString());
			ObjectFlags[]? objectFlags = Enum.GetValues<ObjectFlags>();
			for (var i = 0; i < objectFlags.Length; i++)
			{
				ObjectFlags flag = objectFlags[i];
				if (flag == ObjectFlags.None) continue; // All have this :P
				if (Object.ObjectFlags.HasFlag(flag)) metaText.Append($", {flag}");
			}

			// Warning: These objects might not actually be in these layers.
			// If they reported a different value to IsPartOfMapLayer when being added.
			metaText.Append("\nIn Layers: ");
			var idx = 0;
			foreach (int treeLayerId in ObjectMap.GetWorldTree()!.ForEachLayer())
			{
				if (Object.IsPartOfMapLayer(treeLayerId))
				{
					if (idx != 0) metaText.Append(", ");
					metaText.Append(treeLayerId);
					idx++;
				}
			}

			statusLabel.Text = metaText.ToString();

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
					fieldEditorContainer.AddChild(label);

					var editorParent = new UIBaseWindow();
					editorParent.InputTransparent = false;
					editorParent.StretchX = true;
					editorParent.StretchY = true;
					editorParent.MinSize = new Vector2(70, 0);
					editorParent.Anchor = UIAnchor.CenterRight;
					editorParent.ParentAnchor = UIAnchor.CenterRight;
					fieldEditorContainer.AddChild(editorParent);

					IMapEditorGeneric? editor = AddEditorForField(field);
					if (editor != null)
					{
						object? propertyValue = field.ReflectionInfo.GetValue(Object);
						editor.SetValue(propertyValue);
						editor.SetCallbackValueChanged((newValue) =>
						{
							ApplyObjectChange(field, newValue);
						});
						editorParent.AddChild((UIBaseWindow) editor);
					}
					
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

		private IMapEditorGeneric AddEditorForField(XMLFieldHandler field)
		{
			// todo: Insert switch for different types based on field

			object? propertyValue = field.ReflectionInfo.GetValue(Object);

			if (field.TypeHandler.Type == typeof(Vector2))
			{
				return new MapEditorFloat2();
			}
			if (field.TypeHandler.Type == typeof(float))
			{
				return new MapEditorFloat();
			}
			if (field.TypeHandler.Type == typeof(Vector3))
			{
				return new MapEditorFloat3();
			}
			if (field.TypeHandler.Type == typeof(string))
			{
				return new MapEditorString();
			}

			var editorBg = new UISolidColor();
			editorBg.StretchX = true;
			editorBg.StretchY = true;
			editorBg.WindowColor = Color.Black * 0.7f;
			editorBg.Id = "EditorFieldEditor";
			editorBg.InputTransparent = false;
			//fieldEditor.AddChild(editorBg);

			//object? propertyValue = field.ReflectionInfo.GetValue(Object);
			var defaultTextValue = (propertyValue ?? "null").ToString()!;
			string editorValue = defaultTextValue;
			if (propertyValue == null) editorValue = "";

			var textInput = new UITextInput();
			textInput.Text = editorValue;
			textInput.WindowColor = MapEditorColorPalette.TextColor;
			textInput.FontFile = "Editor/UbuntuMono-Regular.ttf";
			textInput.FontSize = MapEditorColorPalette.EditorButtonTextSize;
			textInput.Margins = new Rectangle(2, 1, 2, 1);
			textInput.IgnoreParentColor = true;
			textInput.SizeOfText = true;
			textInput.MinSize = new Vector2(70, 0);

			_editConfirmCallback = () =>
			{
				string? text = textInput.Text;
				if (field.TypeHandler.Type == typeof(string)) ApplyObjectChange(field, text);
			};

			editorBg.AddChild(textInput);

			return null;
		}

		private void FieldExitEditor()
		{
			if (_openEditorHandler == null || _openEditor == null) return;

			UIBaseWindow? button = _openEditor.GetWindowById("EditorFieldEditor");
			Debug.Assert(button != null);
			if (button != null) _openEditor.RemoveChild(button);

			SpawnEditorButton(_openEditorHandler, _openEditor);
			_editConfirmCallback = null;
		}

		private void SpawnEditorButton(XMLFieldHandler field, UIBaseWindow fieldEditor)
		{
			//AddEditorForField(field, fieldEditor);
		}

		public void ApplyObjectChange(XMLFieldHandler field, object value)
		{
			GameObject2D oldObject = Object;
			Map2D objectMap = oldObject.Map;

			Debug.Assert(objectMap != null);

			// Remove the object UndoAction and remove the object from the map.
			_objectChangedCallback(Object);
			objectMap.RemoveObject(Object, true);

			// Serialize it and deserialize it to copy it.
			string? objectAsData = XMLFormat.To(oldObject);
			var newObject = XMLFormat.From<GameObject2D>(objectAsData);

			// Change the property and add the object to map.
			field.ReflectionInfo.SetValue(newObject, value);
			objectMap.AddObject(newObject);

			// Now the object has been copied with the changed property and re-added.
			// This guarantees that objects dont have to handle properties changing after the init.
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