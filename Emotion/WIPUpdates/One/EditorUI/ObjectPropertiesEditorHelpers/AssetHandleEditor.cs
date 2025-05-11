using Emotion.Game.World.Editor;
using Emotion.IO;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class AssetHandleEditor<T> : TypeEditor where T : Asset, new()
{
    private SerializableAsset<T>? _objectEditting = null;

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
            Margins = new Primitives.Rectangle(0, 0, 5, 0)
        };
        container.AddChild(label);

        var inputBackground = new UISolidColor
        {
            WindowColor = Color.Black * 0.5f,
            Paddings = new Primitives.Rectangle(5, 3, 5, 3)
        };
        container.AddChild(inputBackground);

        UITextInput2 input = new UITextInput2
        {
            Id = "TextInput",

            FontSize = MapEditorColorPalette.EditorButtonTextSize,
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
                var platform = Engine.Host;
                if (platform is DesktopPlatform winPl)
                {
                    winPl.DeveloperMode_SelectFileNative<T>((file) =>
                    {
                        if (_objectEditting != null)
                        {
                            _objectEditting.Name = file?.Name;
                            OnValueChanged(_objectEditting);
                            UpdateTextInput();
                        }
                    });
                }

                //var explorer = new FilePicker<TextureAsset>((file) => NewEntity(file));
                //Parent!.AddChild(explorer);
            }
        };
        container.AddChild(browse);
    }

    public override void SetValue(object? value)
    {
        if (value is SerializableAsset<T> serializedHandle)
            _objectEditting = serializedHandle;

        UpdateTextInput();
    }

    private void UpdateTextInput()
    {
        UITextInput2? textInput = GetWindowById<UITextInput2>("TextInput");
        AssertNotNull(textInput);
        textInput.Text = _objectEditting?.Name ?? string.Empty;
    }
}
