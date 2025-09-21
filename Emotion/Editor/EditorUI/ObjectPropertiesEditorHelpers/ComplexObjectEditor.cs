#nullable enable

using Emotion.Game.Systems.UI;
using Emotion.Editor.EditorUI.Components;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector;

namespace Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;

public abstract class ComplexObjectEditor : TypeEditor
{
    public abstract TypeEditor? GetEditorForProperty(string propertyName);

    public abstract TypeEditor GetNestedEditor(ComplexTypeHandlerMemberBase member);

    public abstract ComplexTypeHandlerMemberBase? GetMemberForEditor(TypeEditor typeEditor);
}

public class ComplexObjectEditor<T> : ComplexObjectEditor
{
    private T? _value;

    protected Dictionary<string, TypeEditor> _memberToEditor = new();
    protected List<(ComplexTypeHandlerMemberBase, TypeEditor)> _editors = new();

    private EditorScrollArea _scroll;
    public UIBaseWindow EditorList;

    // Communication with the object property editor
    private ObjectPropertyWindow? _objEdit;

    public ComplexObjectEditor()
    {
        _scroll = new EditorScrollArea()
        {
            AutoHideScrollY = true,
            Id = "EditorScrollArea",
        };
        AddChild(_scroll);

        EditorList = new UIBaseWindow()
        {
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 5),
            Paddings = new Rectangle(10, 5, 10, 5),
            Id = "EditorListParent"
        };
        _scroll.AddChildInside(EditorList);
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);
        SpawnEditors();

        _objEdit = GetParentOfKind<ObjectPropertyWindow>();
    }

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);
        EngineEditor.UnregisterForObjectChanges(this);
    }

    public override void SetValue(object? obj)
    {
        if (obj is T objAsT)
            _value = objAsT;
        else
            obj = null;

        EngineEditor.UnregisterForObjectChanges(this);
        if (obj != null)
            EngineEditor.RegisterForObjectChanges(obj, (_) => RefreshAllMemberValues(), this);

        SpawnEditors();
    }

    protected void SpawnEditors()
    {
        if (Controller == null) return;

        EditorList.ClearChildren();
        _memberToEditor.Clear();
        _editors.Clear();

        // todo
        if (_value == null) return;

        IGenericReflectorComplexTypeHandler? typeHandler = ReflectorEngine.GetComplexTypeHandler<T>();
        if (typeHandler == null)
            return;

        IEnumerable<ComplexTypeHandlerMemberBase> complexTypeMembers = typeHandler.GetMembersDeep();
        foreach (ComplexTypeHandlerMemberBase member in complexTypeMembers)
        {
            if (member.HasAttribute<DontShowInEditorAttribute>() != null) continue;

            IGenericReflectorTypeHandler? memberHandler = member.GetTypeHandler();
            TypeEditor? editor = memberHandler?.GetEditor();
            if (editor != null)
            {
                if (editor is ComplexObjectEditor complexEditor)
                    editor = complexEditor.GetNestedEditor(member);

                editor.SetCallbackOnValueChange((newValue) =>
                {
                    if (_value is ValueType)
                    {
                        _value = (T?)member.SetValueInComplexObjectAndReturnParent(_value, newValue);
                        AssertNotNull(_value);
                        OnValueChanged(_value);
                    }
                    else
                    {
                        member.SetValueInComplexObject(_value, newValue);
                        if(_objEdit != null)
                            _objEdit.NotifyPropertyChangedThroughStack();
                        else
                            EngineEditor.ReportChange_ObjectProperty(_value, member.Name, _value, newValue, this);
                    }
                });

                _memberToEditor.Add(member.Name, editor);
                _editors.Add((member, editor));

                bool verticalLabel = editor is ListEditor;
                if (verticalLabel) editor.MinSizeY = 200;

                var editorWithlabel = WrapWithLabel($"{member.Name}:", editor, verticalLabel);
                EditorList.AddChild(editorWithlabel);
            }
            else
            {
                EditorList.AddChild(new EditorLabel($"{member.Name}: [No handler for type - {member.Type.Name}]"));
            }
        }

        if (_value is IObjectEditorExtendedFunctionality<T> ext)
            ext.OnAfterEditorsSpawn(this);

        RefreshAllMemberValues();
    }

    private void RefreshAllMemberValues()
    {
        if (_value == null) return;
        foreach ((ComplexTypeHandlerMemberBase member, TypeEditor editor) in _editors)
        {
            if (member.GetValueFromComplexObject(_value, out object? readValue))
                editor.SetValue(readValue);//, _member?.Append(member));
        }
    }

    #region Public API

    public override TypeEditor? GetEditorForProperty(string propertyName)
    {
        _memberToEditor.TryGetValue(propertyName, out TypeEditor? val);
        return val;
    }

    public override ComplexTypeHandlerMemberBase? GetMemberForEditor(TypeEditor typeEditor)
    {
        foreach ((ComplexTypeHandlerMemberBase member, TypeEditor editor) in _editors)
        {
            if (editor == typeEditor)
                return member;
        }
        return null;
    }

    #endregion

    #region Nested Editor

    public class NestedComplexObjectEditor : TypeEditor
    {
        private EditorButton _button;
        private ComplexTypeHandlerMemberBase _member;

        public NestedComplexObjectEditor(ComplexTypeHandlerMemberBase member)
        {
            _member = member;

            var editButton = new EditorButton()
            {
                GrowX = true,
                OnClickedProxy = (_) => OpenEditor()
            };
            var padding = editButton.Paddings;
            padding.Width = 30;
            editButton.Paddings = padding;
            AddChild(editButton);
            _button = editButton;

            var arrowSquare = new UIBaseWindow()
            {
                GrowX = false,
                GrowY = false,
                MinSizeY = 23,
                MinSizeX = 29,
                AnchorAndParentAnchor = UIAnchor.TopRight,
            };
            AddChild(arrowSquare);

            var arrowIcon = new UITexture()
            {
                Smooth = true,
                TextureFile = "Editor/Edit.png",
                ImageScale = new Vector2(0.6f),
                Offset = new Vector2(0, 4),
                AnchorAndParentAnchor = UIAnchor.CenterCenter
            };
            arrowSquare.AddChild(arrowIcon);
        }

        public override void SetValue(object? value)
        {
            _button.Text = value == null ? "<null>" : value.ToString();
        }

        public void OpenEditor()
        {
            var objEdit = GetParentOfKind<ObjectPropertyWindow>();
            objEdit?.AddEditPage(_member);
        }
    }

    public override TypeEditor GetNestedEditor(ComplexTypeHandlerMemberBase member)
    {
        return new NestedComplexObjectEditor(member);
    }

    #endregion
}