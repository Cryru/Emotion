using Emotion.Game.Systems.UI;

#nullable enable

namespace Emotion.Editor.EditorUI.GridEditor;

public class GridEditorToolButton : UICallbackButton
{
    public GridEditorWindow Editor;
    public GridEditorTool Tool;

    private UISolidColor _windowBackground;

    public GridEditorToolButton(GridEditorWindow editor, GridEditorTool tool)
    {
        Editor = editor;
        Tool = tool;

        GrowX = false;

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
            _windowBackground.WindowColor = GridEditorColorPalette.ToolButtonSelectedColor;
        else if (isRollover)
            _windowBackground.WindowColor = GridEditorColorPalette.ToolButtonRolloverColor;
        else
            _windowBackground.WindowColor = GridEditorColorPalette.ToolButtonColor;
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
