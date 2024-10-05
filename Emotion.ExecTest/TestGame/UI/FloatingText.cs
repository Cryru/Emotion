using Emotion.Game.Time;
using Emotion.Utility;

namespace Emotion.ExecTest.TestGame.UI;

public class FloatingText
{
    public string Text;
    public After Timer;
    public Color Color;
    public Vector3 Position;

    public FloatingText(string text, Vector3 position, Color? color = null)
    {
        Text = text;
        Timer = new After(1000);
        Color = color ?? Color.White;
        Position = position + new Vector3(Helpers.GenerateRandomNumber(-4, 4), 0, 0);
    }
}