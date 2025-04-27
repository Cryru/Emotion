using Emotion.Editor;
using Emotion.IO;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class ObjectPropertyWindow : EditorScrollArea
{
    public object? ObjectBeingEdited { get; protected set; }
    protected Type? _type;

    public ObjectPropertyWindow()
    {
        AutoHideScrollY = true;

        UIBaseWindow editorList = new()
        {
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 5),
            Paddings = new Rectangle(10, 5, 10, 5),
            Id = "EditorListParent"
        };
        AddChildInside(editorList);
    }

    public void SetEditor(object? obj)
    {
        ObjectBeingEdited = obj;
        _type = obj?.GetType();
        SpawnEditors();
    }

    protected void SpawnEditors()
    {
        UIBaseWindow? editorList = GetWindowById("EditorListParent");
        AssertNotNull(editorList);

        editorList.ClearChildren();

        if (_type == null) return;

        // todo
        if (ObjectBeingEdited == null) return;

        IGenericReflectorTypeHandler? typeHandler = ReflectorEngine.GetTypeHandler(_type);

        if (typeHandler == null)
        {
            EditorLabel label = new EditorLabel();
            label.Text = $"The object attempting to be edited is non-editable.";
            editorList.AddChild(label);
            return;
        }

        // todo: implement for non complex members
        // primitives, lists, etc.

        if (typeHandler is IGenericReflectorComplexTypeHandler complex)
        {
            IEnumerable<ComplexTypeHandlerMember> complexTypeMembers = complex.GetMembersDeep();

            foreach (ComplexTypeHandlerMember member in complexTypeMembers)
            {
                if (member.HasAttribute<DontShowInEditorAttribute>() != null) continue;

                IGenericReflectorTypeHandler? memberHandler = member.GetTypeHandler();

                TypeEditor? editorFromReflector = memberHandler?.GetEditor();
                if (editorFromReflector != null)
                {
                    var editorWithlabel = new EditorWithLabel(editorFromReflector, ObjectBeingEdited, member);
                    editorList.AddChild(editorWithlabel);
                }
                else if (memberHandler?.Type == typeof(SerializableAsset<TextureAsset>))
                {
                    AssetHandleEditor<TextureAsset> newEditor = new AssetHandleEditor<TextureAsset>();

                    SerializableAsset<TextureAsset> handleValue;
                    if (member.GetValueFromComplexObject(ObjectBeingEdited, out object? memberValue) && memberValue is SerializableAsset<TextureAsset> readMember)
                    {
                        handleValue = readMember;
                    }
                    else
                    {
                        handleValue = new SerializableAsset<TextureAsset>();
                        member.SetValueInComplexObject(ObjectBeingEdited, handleValue);
                    }

                    newEditor.SetEditor(member.Name, handleValue);
                    editorList.AddChild(newEditor);
                }
                else
                {
                    editorList.AddChild(new EditorLabel($"{member.Name}: [No handler for type - {member.Type.Name}]"));
                }
            }
        }
    }
}
