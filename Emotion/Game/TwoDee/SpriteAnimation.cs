using Emotion.IO;

namespace Emotion.Game.TwoDee;

#nullable enable

public class SpriteAnimationBodyPart
{
    public string Name = "Unnamed Part";

    public RectangleAnchor AttachSpot = RectangleAnchor.CenterCenter;
    public string AttachToPoint = "origin";

    public int TimeBetweenFrames = 500;
    public List<SpriteAnimationFrame> Frames = new List<SpriteAnimationFrame>();

    public bool Visible = true;

    public float Duration
    {
        get => Frames.Count * TimeBetweenFrames;
    }

    public override string ToString()
    {
        return Name;
    }
}

public class SpriteAnimation
{
    public string Name = "Unnamed Animation";

    public float TotalDuration
    {
        get
        {
            float myDuration = 1;
            foreach (SpriteAnimationBodyPart part in Parts)
            {
                if (part.Duration > myDuration)
                    myDuration = part.Duration;
            }
            return myDuration;
        }
    }

    public List<SpriteAnimationBodyPart> Parts = new List<SpriteAnimationBodyPart>();

    public IEnumerable<SpriteAnimationBodyPart> ForEachPart()
    {
        for (int i = 0; i < Parts.Count; i++)
        {
            var part = Parts[i];
            yield return part;
        }
    }

    public override string ToString()
    {
        return Name;
    }
}