#nullable enable

using Emotion;

namespace Emotion.Graphics.TwoDee;

public class SpriteEntity
{
    public static SpriteEntity MissingEntity = new SpriteEntity()
    {
        Name = "Missing Entity"
    };

    public string Name = "Unnamed Entity";
    public bool PixelArt;

    public List<SpriteAnimation> Animations = new();

    public void GetBounds(SpriteAnimation? animation, out Rectangle baseRect)
    {
        baseRect = new Rectangle(0, 0, 1, 1);
        if (animation == null) return;

        foreach (SpriteAnimationBodyPart part in animation.ForEachPart())
        {
            if (part.AttachToPoint != "origin") continue;
            foreach (SpriteAnimationFrame frame in part.Frames)
            {
                Rectangle frameRect = frame.GetBoundingRect(part);
                baseRect = Rectangle.Union(baseRect, frameRect);
            }
        }
    }
}

