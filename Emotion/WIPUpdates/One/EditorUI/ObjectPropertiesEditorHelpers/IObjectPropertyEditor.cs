using Emotion.Standard.Reflector.Handlers;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public interface IObjectPropertyEditor
{
    public void SetEditor(object parentObj, ComplexTypeHandlerMember memberHandler);
}
