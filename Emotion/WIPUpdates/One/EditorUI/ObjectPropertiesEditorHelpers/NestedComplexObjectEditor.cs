#nullable enable

using Emotion.WIPUpdates.One.EditorUI.Components;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class NestedComplexObjectEditor : TypeEditor
{
    private object? _value;
    private bool _open;

    private ObjectPropertyWindow? _props;
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
            _props = new ObjectPropertyWindow()
            {
                ExpandY = true,
                MaxSizeY = 200
            };
            _props.SetEditor(_value);
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
