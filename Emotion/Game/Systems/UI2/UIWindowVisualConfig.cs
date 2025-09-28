#nullable enable

namespace Emotion.Game.Systems.UI2;

public struct UIWindowVisualConfig
{
    public Color BackgroundColor = Color.White.SetAlpha(0);

    public int Border = 0;
    public Color BorderColor = Color.Black;

    public bool Visible = true;

    /// <summary>
    /// Whether to consider this window as part of the layout when invisible.
    /// Matters only within lists.
    /// </summary>
    public bool DontTakeSpaceWhenHidden
    {
        get => _dontTakeSpaceWhenHidden;
        set
        {
            if (value == _dontTakeSpaceWhenHidden) return;
            _dontTakeSpaceWhenHidden = value;
            InvalidateLayout();
        }
    }

    private bool _dontTakeSpaceWhenHidden;

    public UIWindowVisualConfig()
    {
    }

    private void InvalidateLayout()
    {

    }

    public override string ToString()
    {
        return "Visuals";
    }
}