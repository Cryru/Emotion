namespace Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;

public interface IObjectEditorExtendedFunctionality<T>
{
    public void OnAfterEditorsSpawn(ComplexObjectEditor<T> editor);
}
