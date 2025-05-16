using Emotion.Editor;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.Standard.Reflector;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using System.Reflection.Metadata;
using System.ComponentModel;
using System.Reflection.Emit;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public abstract class ComplexObjectEditor : TypeEditor
{
    public abstract TypeEditor? GetEditorForProperty(string propertyName);
}

public class ComplexObjectEditor<T> : ComplexObjectEditor
{
    public object? ObjectBeingEdited { get; protected set; }

    protected Type _type = typeof(T);

    protected Dictionary<string, TypeEditor> _memberToEditor = new();
    protected List<(ComplexTypeHandlerMemberBase, TypeEditor)> _editors = new();

    private EditorScrollArea _scroll;
    public UIBaseWindow EditorList;

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

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);
        EngineEditor.UnregisterForObjectChanges(this);
    }

    public override void SetValue(object? obj)
    {
        ObjectBeingEdited = obj;

        EngineEditor.UnregisterForObjectChanges(this);
        if (obj != null)
            EngineEditor.RegisterForObjectChanges(obj, (_) => RefreshAllMemberValues(), this);

        SpawnEditors();
    }

    protected void SpawnEditors()
    {
        EditorList.ClearChildren();
        _memberToEditor.Clear();
        _editors.Clear();

        // todo
        if (ObjectBeingEdited == null) return;

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
                editor.SetCallbackOnValueChange((newValue) =>
                {
                    member.SetValueInComplexObject(ObjectBeingEdited, newValue);
                    EngineEditor.ObjectChanged(ObjectBeingEdited, ObjectChangeType.ComplexObject_PropertyChanged, this);
                    OnValueChanged(ObjectBeingEdited);
                });

                if (editor is ListEditor)
                    editor.MinSizeY = 200;

                editor.SetParentObject(ObjectBeingEdited);

                bool verticalLabel = editor is NestedComplexObjectEditor || editor is ListEditor;
                var editorWithlabel = TypeEditor.WrapWithLabel(member.Name + ":", editor, verticalLabel);
                EditorList.AddChild(editorWithlabel);

                _memberToEditor.Add(member.Name, editor);
                _editors.Add((member, editor));
            }
            else
            {
                EditorList.AddChild(new EditorLabel($"{member.Name}: [No handler for type - {member.Type.Name}]"));
            }
        }

        if (ObjectBeingEdited is IObjectEditorExtendedFunctionality<T> ext)
            ext.OnAfterEditorsSpawn(this);

        RefreshAllMemberValues();
    }

    private void RefreshAllMemberValues()
    {
        if (ObjectBeingEdited == null) return;
        foreach ((ComplexTypeHandlerMemberBase member, TypeEditor editor) in _editors)
        {
            if (member.GetValueFromComplexObject(ObjectBeingEdited, out object? readValue))
                editor.SetValue(readValue);
        }
    }

    #region Public API

    public override TypeEditor? GetEditorForProperty(string propertyName)
    {
        _memberToEditor.TryGetValue(propertyName, out TypeEditor? val);
        return val;
    }

    #endregion
}