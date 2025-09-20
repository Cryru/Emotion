#nullable enable

namespace Emotion.Game.Systems.UI2;

public class O_UIWindowVisuals
{
    public Color Color = Color.White.SetAlpha(0);

    public bool Visible = true;

    public override string ToString()
    {
        return "Visuals";
    }
}