#nullable enable

using Emotion.Game.Systems.UI;
using Emotion.Editor.EditorUI.Components;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector;
using Emotion.Game.Systems.UI2.Editor;

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
            Name = "EditorScrollArea",
        };
        AddChild(_scroll);

        EditorList = new UIBaseWindow()
        {
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 5),
            Paddings = new Rectangle(10, 5, 10, 5),
            Name = "EditorListParent"
        };
        _scroll.AddChildInside(EditorList);
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        SpawnEditors();

        _objEdit = GetParentOfKind<ObjectPropertyWindow>();
    }

    protected override void OnClose()
    {
        base.OnClose();
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
        if (State != UIWindowState.Open) return;

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
                        if (_objEdit != null)
                            _objEdit.NotifyPropertyChangedThroughStack();
                        else
                            EngineEditor.ReportChange_ObjectProperty(_value, member.Name, _value, newValue, this);
                    }
                });

                _memberToEditor.Add(member.Name, editor);
                _editors.Add((member, editor));

                bool verticalLabel = editor is ListEditor;
                if (verticalLabel) editor.Layout.SizingY = UISizing.Fixed(200);

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
            UISpacing padding = editButton.Layout.Padding;
            padding.RightBottom.X = 30;
            editButton.Layout.Padding = padding;
            AddChild(editButton);
            _button = editButton;

            var arrowSquare = new UIBaseWindow()
            {
                Layout =
                {
                    SizingX = UISizing.Fixed(29),
                    SizingY = UISizing.Fixed(23),
                    AnchorAndParentAnchor = UIAnchor.TopRight,
                }
            };
            AddChild(arrowSquare);

            UIPicture arrowIcon = new()
            {
                Layout =
                {
                    AnchorAndParentAnchor = UIAnchor.CenterCenter,
                    Offset = new IntVector2(0, 4)
                },

                Smooth = true,
                Texture = "Editor/Edit.png",
                ImageScale = new Vector2(0.6f),
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