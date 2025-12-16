#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Editor.EditorUI.Components;

namespace Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;

public class AssetHandleEditor<T> : TypeEditor where T : Asset, new()
{
    private SerializableAsset<T>? _objectEditing = null;

    public AssetHandleEditor()
    {
        GrowY = false;

        UIBaseWindow container = new UIBaseWindow
        {
            Layout =
            {
                LayoutMethod = UILayoutMethod.HorizontalList(0)
            }
        };
        AddChild(container);

        EditorLabel label = new EditorLabel
        {
            Name = "Label",
            Margins = new Rectangle(0, 0, 5, 0)
        };
        container.AddChild(label);

        var inputBackground = new UISolidColor
        {
            WindowColor = Color.Black * 0.5f,
            Paddings = new Rectangle(5, 3, 5, 3)
        };
        container.AddChild(inputBackground);

        UITextInput input = new UITextInput
        {
            Name = "TextInput",
            Layout =
            {
                MinSizeX = 100,
                AnchorAndParentAnchor = UIAnchor.CenterLeft
            },
            FontSize = EditorColorPalette.EditorButtonTextSize,
            IgnoreParentColor = true
        };
        inputBackground.AddChild(input);

        EditorButton browse = new EditorButton
        {
            Text = "...",
            OnClickedProxy = (_) =>
            {
                FilePicker<T>.SelectFile(this, (file) =>
                {
                    if (_objectEditing == null) return;

                    // Create a new handle, we treat these as immutable
                    _objectEditing = file?.Name;
                    OnValueChanged(_objectEditing);
                    UpdateTextInput();
                });
            }
        };
        container.AddChild(browse);
    }

    public override void SetValue(object? value)
    {
        if (value is SerializableAsset<T> serializedHandle)
            _objectEditing = serializedHandle;

        UpdateTextInput();
    }

    private void UpdateTextInput()
    {
        UITextInput? textInput = GetWindowById<UITextInput>("TextInput");
        AssertNotNull(textInput);
        textInput.Text = _objectEditing?.Name ?? string.Empty;
    }
}
