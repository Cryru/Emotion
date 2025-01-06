using Emotion.Game.World2D.Editor;
using Emotion.UI;

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
            WindowColor = Editor.CurrentTool == Tool ? TileEditorColorPalette.ToolButtonSelectedColor : TileEditorColorPalette.ToolButtonColor,
            Id = "Background"
        };
        AddChild(windowBackground);
        _windowBackground = windowBackground;

        var iconUI = new UITexture
        {
            TextureFile = $"Editor/{Tool}.png",
            Smooth = true,
            ImageScale = new Vector2(1f)
        };
        AddChild(iconUI);
    }

    protected override void OnClicked()
    {
        Editor.SetCurrentTool(Tool);
    }

    public override void OnMouseEnter(Vector2 _)
    {
        base.OnMouseEnter(_);

        bool isSelected = Editor.CurrentTool == Tool;
        _windowBackground.WindowColor = isSelected ? TileEditorColorPalette.ToolButtonSelectedColor : TileEditorColorPalette.ToolButtonRolloverColor;
    }

    public override void OnMouseLeft(Vector2 mousePos)
    {
        base.OnMouseLeft(mousePos);

        bool isSelected = Editor.CurrentTool == Tool;
        _windowBackground.WindowColor = isSelected ? TileEditorColorPalette.ToolButtonSelectedColor : TileEditorColorPalette.ToolButtonColor;
    }
}
