#nullable enable

using System.Xml.Linq;

namespace Emotion.Game.World.TwoDee;

public class SpriteEntity
{
    public static SpriteEntity MissingEntity = new SpriteEntity()
    {
        Name = "Missing Entity"
    };

    public string Name = "Unnamed Entity";
    public bool PixelArt;

    public List<SpriteAnimation> Animations = new();

    public SpriteAnimation? GetAnimation(string? animationName, bool returnFirstOnFail = false)
    {
        if (animationName == null || Animations == null || Animations.Count == 0)
            return null;

        for (int i = 0; i < Animations.Count; i++)
        {
            SpriteAnimation anim = Animations[i];
            if (anim.Name == animationName)
                return anim;
        }

        return returnFirstOnFail ? Animations[0] : null;
    }

    public Rectangle GetBounds(string animationName)
    {
        SpriteAnimation? animation = GetAnimation(animationName);
        if (animation == null)
            return new Primitives.Rectangle(0, 0, 1, 1);

        Vector2 totalMin = Vector2.Zero;
        Vector2 totalMax = Vector2.Zero;
        foreach (SpriteAnimationBodyPart part in animation.ForEachPart())
        {
            if (part.AttachToPoint != "origin") continue;
            foreach (SpriteAnimationFrame frame in part.Frames)
            {
                Rectangle frameRect = frame.GetBoundingRect(part);
                frameRect.GetMinMaxPoints(out Vector2 min, out Vector2 max);
                totalMin = Vector2.Min(min, totalMin);
                totalMax = Vector2.Max(max, totalMax);
            }
        }
        return Rectangle.FromMinMaxPointsChecked(totalMin, totalMax);
    }
}

