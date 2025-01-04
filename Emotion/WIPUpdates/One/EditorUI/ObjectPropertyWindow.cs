using Emotion.IO;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Helpers;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI;

public class ObjectPropertyWindow : UIBaseWindow
{
    public object? Object;
    protected Type? _type;

    public void SetObject(object obj)
    {
        Object = obj;
        _type = obj.GetType();

        SpawnEditors();
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIBaseWindow editorList = new()
        {
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 5),
            Id = "EditorListParent"
        };
        AddChild(editorList);

        SpawnEditors();
    }

    protected void SpawnEditors()
    {
        UIBaseWindow? editorList = GetWindowById("EditorListParent");
        AssertNotNull(editorList);

        editorList.ClearChildren();

        if (_type == null) return;

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
                    newEditor.SetEditor(member.Name);
                    editorList.AddChild(newEditor);
                }
            }
        }
    }
}
