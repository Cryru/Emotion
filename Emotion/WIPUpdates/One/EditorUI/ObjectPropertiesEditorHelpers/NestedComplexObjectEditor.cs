#nullable enable

using Emotion.WIPUpdates.One.EditorUI.Components;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public abstract class NestedComplexObjectEditor : TypeEditor
{

}

public class NestedComplexObjectEditor<T> : NestedComplexObjectEditor
{
    private object? _value;
    private bool _open;

    private ComplexObjectEditor<T>? _props;
    private EditorButton _button;

    public NestedComplexObjectEditor()
    {
        LayoutMode = UI.LayoutMode.VerticalList;
        ListSpacing = new Vector2(0, 5);

        var openCloseButton = new EditorButton("Expand");
        openCloseButton.OnClickedProxy = (_) => SetOpen(!_open);
        AddChild(openCloseButton);
        _button = openCloseButton;
    }

    private void SetOpen(bool editorOpen)
    {
        if (editorOpen == _open) return;
        if (_value == null) return;

        if (editorOpen)
        {
            _props = new ComplexObjectEditor<T>()
            {
                MaxSizeY = 200
            };

            var list = _props.GetWindowById<EditorScrollArea>("EditorScrollArea");
            if (list != null)
            {
                list.ExpandY = true;
            }

            _props.SetValue(_value);
            AddChild(_props);
        }
        else
        {
            _props?.Close();
            _props = null;
        }

        _open = editorOpen;

        _button.Text = _open ? "Close" : "Expand";
    }

    public override void SetValue(object? value)
    {
        _value = value;
    }
}
