using Emotion.UI;
using Emotion.WIPUpdates.One.Editor2D.TileEditor.Tools;

#nullable enable

namespace Emotion.WIPUpdates.One.Editor2D.TileEditor;

public class TileEditorToolButton : UICallbackButton
{
    public TileEditorWindow Editor;
    public TileEditorTool Tool;

    private UISolidColor _windowBackground;

    public TileEditorToolButton(TileEditorWindow editor, TileEditorTool tool)
    {
        Editor = editor;
        Tool = tool;

        FillX = false;

        var windowBackground = new UISolidColor
        {
            Id = "Background"
        };
        AddChild(windowBackground);
        _windowBackground = windowBackground;

        var iconUI = new UITexture
        {
            TextureFile = $"Editor/{Tool.Name}.png",
            Smooth = true,
            ImageScale = new Vector2(1f)
        };
        AddChild(iconUI);

        UpdateStyle();
    }

    protected override void OnClicked()
    {
        Editor.SetCurrentTool(Tool);
    }

    public void UpdateStyle()
    {
        bool isSelected = Editor.CurrentTool == Tool;
        bool isRollover = MouseInside;
        if (isSelected)
            _windowBackground.WindowColor = TileEditorColorPalette.ToolButtonSelectedColor;
        else if (isRollover)
            _windowBackground.WindowColor = TileEditorColorPalette.ToolButtonRolloverColor;
        else
            _windowBackground.WindowColor = TileEditorColorPalette.ToolButtonColor;
    }

    public override void OnMouseEnter(Vector2 _)
    {
        base.OnMouseEnter(_);
        UpdateStyle();
    }

    public override void OnMouseLeft(Vector2 mousePos)
    {
        base.OnMouseLeft(mousePos);
        UpdateStyle();
    }
}
