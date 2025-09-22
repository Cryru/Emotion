#nullable enable

namespace Emotion.Game.Systems.UI2;

public struct O_UIWindowVisuals
{
    public Color Color = Color.White.SetAlpha(0);
    public float Border = 0;
    public Color BorderColor = Color.Black;

    public bool Visible = true;

    public O_UIWindowVisuals()
    {
    }

    public override string ToString()
    {
        return "Visuals";
    }
}