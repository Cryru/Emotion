#nullable enable

using Emotion.Common.Serialization;
using Emotion.UI;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public abstract class ObjectPropertyEditor : UIBaseWindow
{
    [DontSerialize]
    private Action<object?>? _onValueChanged;

    public abstract void SetValue(object? value);

    public void SetCallbackOnValueChange(Action<object?> onValueChanged)
    {
        _onValueChanged = onValueChanged;
    }

    protected void OnValueChanged(object? newValue)
    {
        _onValueChanged?.Invoke(newValue);
    }
}
