using Emotion.Editor;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.Standard.Reflector;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

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

    private EditorScrollArea _scroll;
    private UIBaseWindow _list;

    public ComplexObjectEditor()
    {
        _scroll = new EditorScrollArea()
        {
            AutoHideScrollY = true,
            Id = "EditorScrollArea",
        };
        AddChild(_scroll);

        _list = new UIBaseWindow()
        {
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 5),
            Paddings = new Rectangle(10, 5, 10, 5),
            Id = "EditorListParent"
        };
        _scroll.AddChildInside(_list);
    }

    public override void SetValue(object? obj)
    {
        ObjectBeingEdited = obj;
        SpawnEditors();
    }

    protected void SpawnEditors()
    {
        _list.ClearChildren();
        _memberToEditor.Clear();

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
                var editorWithlabel = new ObjectPropertyEditor(editor, ObjectBeingEdited, member);
                _list.AddChild(editorWithlabel);

                _memberToEditor.Add(member.Name, editor);
            }
            else
            {
                _list.AddChild(new EditorLabel($"{member.Name}: [No handler for type - {member.Type.Name}]"));
            }
        }
    }

    public override TypeEditor? GetEditorForProperty(string propertyName)
    {
        _memberToEditor.TryGetValue(propertyName, out TypeEditor? val);
        return val;
    }
}