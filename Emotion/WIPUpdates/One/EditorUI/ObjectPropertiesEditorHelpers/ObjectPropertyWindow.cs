using Emotion.Editor;
using Emotion.IO;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class ObjectPropertyWindow : UIScrollArea
{
    public object? Object;
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

    public void SetEditor(object obj)
    {
        Object = obj;
        _type = obj.GetType();
        SpawnEditors();
    }

    protected void SpawnEditors()
    {
        UIBaseWindow? editorList = GetWindowById("EditorListParent");
        AssertNotNull(editorList);

        editorList.ClearChildren();

        if (_type == null) return;

        // todo
        if (Object == null) return;

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

                if (memberHandler is StringTypeHandler)
                {
                    var stringEditor = new StringEditor();
                    stringEditor.SetEditor(Object, member);
                    editorList.AddChild(stringEditor);
                }
                else if (memberHandler?.Type == typeof(SerializableAssetHandle<TextureAsset>))
                {
                    AssetHandleEditor<TextureAsset> newEditor = new AssetHandleEditor<TextureAsset>();

                    SerializableAssetHandle<TextureAsset> handleValue;
                    if (member.GetValueFromComplexObject(Object, out object? memberValue) && memberValue is SerializableAssetHandle<TextureAsset> readMember)
                    {
                        handleValue = readMember;
                    }
                    else
                    {
                        handleValue = new SerializableAssetHandle<TextureAsset>();
                        member.SetValueInComplexObject(Object, handleValue);
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
