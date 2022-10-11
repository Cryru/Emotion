#region Using

using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Primitives;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D
{
    internal class MapEditorObjectPropertiesPanel : MapEditorPanel
    {
        public GameObject2D Object;

        private List<EditorUtility.TypeAndFieldHandlers> _fields;
        private UIBaseWindow _openEditor;
        private XMLFieldHandler _openEditorHandler;

        public MapEditorObjectPropertiesPanel(GameObject2D obj) : base($"{obj.ObjectName ?? obj.GetHashCode().ToString()} Properties")
        {
            Object = obj;
            _fields = EditorUtility.GetTypeFields(obj);
        }

        public override void AttachedToController(UIController controller)
        {
            base.AttachedToController(controller);

            UIBaseWindow contentWin = _contentParent;
            contentWin.InputTransparent = false;

            UIBaseWindow innerContainer = new UIBaseWindow();
            innerContainer.StretchX = true;
            innerContainer.StretchY = true;
            innerContainer.InputTransparent = false;
            innerContainer.LayoutMode = LayoutMode.VerticalList;
            innerContainer.ListSpacing = new Vector2(0, 3);
            innerContainer.ChildrenAllSameWidth = true;
            contentWin.AddChild(innerContainer);

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
            listNav.MinSize = new Vector2(100, 200);
            listNav.MaxSize = new Vector2(9999, 200);
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
            scrollBar.MaxSize = new Vector2(5, 999);
            listNav.SetScrollbar(scrollBar);
            listContainer.AddChild(scrollBar);

            for (int i = 0; i < _fields.Count; i++)
            {
                var fieldGroup = _fields[i];

                UIBaseWindow fieldGroupHeaderContainer = new UIBaseWindow();
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
            UIBaseWindow focus = Controller?.InputFocus;
            if (_openEditor != null && focus != null && !focus.IsWithin(this)) FieldExitEditor();

            return base.UpdateInternal();
        }

        private void FieldEnterEditor(XMLFieldHandler field, UIBaseWindow fieldEditor)
        {
            if (_openEditor != null) FieldExitEditor();

            UIController controller = Controller;

            UIBaseWindow button = fieldEditor.GetWindowById("EditorButton");
            Debug.Assert(button != null);
            if (button != null) fieldEditor.RemoveChild(button);

            var textInput = new UITextInput();
            textInput.Text = (field.ReflectionInfo.GetValue(Object) ?? "null").ToString();
            textInput.WindowColor = MapEditorColorPalette.TextColor;
            textInput.FontFile = "Editor/UbuntuMono-Regular.ttf";
            textInput.FontSize = MapEditorColorPalette.EditorButtonTextSize;
            textInput.MinSize = new Vector2(40, 0);
            textInput.Margins = new Rectangle(2, 1, 2, 1);
            textInput.Id = "EditorFieldEditor";

            fieldEditor.AddChild(textInput);
            controller?.SetInputFocus(textInput);

            _openEditor = fieldEditor;
            _openEditorHandler = field;
        }

        private void FieldExitEditor()
        {
            UIBaseWindow button = _openEditor.GetWindowById("EditorFieldEditor");
            Debug.Assert(button != null);
            if (button != null) _openEditor.RemoveChild(button);

            SpawnEditorButton(_openEditorHandler, _openEditor);

            _openEditor = null;
            _openEditorHandler = null;
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
    }
}