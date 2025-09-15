using Emotion.Core.Systems.IO;
using Emotion.Editor.EditorUI.Components;
using Emotion.Game.Systems.UI;
using Emotion.Game.Systems.UI.Text.TextUpdate;

#nullable enable

namespace Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;

public class AssetHandleEditor<T> : TypeEditor where T : Asset, new()
{
    private SerializableAsset<T>? _objectEditing = null;

    public AssetHandleEditor()
    {
        GrowY = false;

        UIBaseWindow container = new UIBaseWindow
        {
            LayoutMode = LayoutMode.HorizontalList
        };
        AddChild(container);

        EditorLabel label = new EditorLabel
        {
            Id = "Label",
            Margins = new Rectangle(0, 0, 5, 0)
        };
        container.AddChild(label);

        var inputBackground = new UISolidColor
        {
            WindowColor = Color.Black * 0.5f,
            Paddings = new Rectangle(5, 3, 5, 3)
        };
        container.AddChild(inputBackground);

        UITextInput2 input = new UITextInput2
        {
            Id = "TextInput",

            FontSize = EditorColorPalette.EditorButtonTextSize,
            MinSizeX = 100,
            AnchorAndParentAnchor = UIAnchor.CenterLeft,
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
        UITextInput2? textInput = GetWindowById<UITextInput2>("TextInput");
        AssertNotNull(textInput);
        textInput.Text = _objectEditing?.Name ?? string.Empty;
    }
}
