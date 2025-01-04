using Emotion.Editor.EditorHelpers;
using Emotion.IO;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Helpers;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class AssetHandleEditor<T> : UIBaseWindow where T : Asset, IAssetWithFileExtensionSupport, new()
{
    public AssetHandleEditor()
    {
        FillY = false;

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

        UITextInput2 input = new UITextInput2();
        container.AddChild(input);

        EditorButton browse = new EditorButton
        {
            Text = "...",
            OnClickedUpProxy = (_) =>
            {
                var platform = Engine.Host;
                if (platform is DesktopPlatform winPl)
                {
                    winPl.DeveloperMode_SelectFileNative<T>((file) =>
                    {
                        bool a = true;
                    });
                }

                //var explorer = new FilePicker<TextureAsset>((file) => NewEntity(file));
                //Parent!.AddChild(explorer);
            }
        };
        container.AddChild(browse);
    }

    public void SetEditor(string labelText)
    {
        EditorLabel label = GetWindowById<EditorLabel>("Label");
        AssertNotNull(label);
        label.Text = labelText + ":";
    }
}
