namespace Emotion.Game.TwoDee;

public class SpriteAnimationFrame
{
    public int TextureId;
    public Rectangle UV;

    public override string ToString()
    {
        return $"{TextureId} - {UV}";
    }
}

