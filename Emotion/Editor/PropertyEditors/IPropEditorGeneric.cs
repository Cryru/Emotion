#region Using

using Emotion.Standard.XML;

#endregion

namespace Emotion.Editor.PropertyEditors;

public interface IPropEditorGeneric
{
    public XMLFieldHandler Field { get; set; }

    public void SetValue(object value);
    public object GetValue();

    public void SetCallbackValueChanged(Action<object> callback);
}