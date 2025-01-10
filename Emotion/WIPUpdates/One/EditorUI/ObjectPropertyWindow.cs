using Emotion.IO;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI;

public class ObjectPropertyWindow : UIScrollArea
{
    public object? Object;
    protected Type? _type;

    public void SetObject(object obj)
    {
        Object = obj;
        _type = obj.GetType();

        AutoHideScrollY = true;

        SpawnEditors();
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIBaseWindow editorList = new()
        {
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 5),
            Paddings = new Primitives.Rectangle(10, 5, 10, 5),
            Id = "EditorListParent"
        };
        AddChildInside(editorList);

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
            label.Text = $"The reflector engine cannot reflect this type :/";
            editorList.AddChild(label);
            return;
        }

        // todo: implement for non complex members
        // primitives, lists, etc.

        if (typeHandler is IGenericReflectorComplexTypeHandler complex)
        {
            ComplexTypeHandlerMember[]? complexTypeMembers = complex.GetMembers();

            foreach (ComplexTypeHandlerMember member in complexTypeMembers)
            {
                IGenericReflectorTypeHandler? memberHandler = member.GetTypeHandler();

                // todo
                if (memberHandler.Type == typeof(SerializableAssetHandle<TextureAsset>))
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
            }
        }
    }

    public static void ObjectChanged(object obj)
    {

    }
}
