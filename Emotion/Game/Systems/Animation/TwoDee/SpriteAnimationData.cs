#nullable enable

namespace Emotion.Game.Systems.Animation.TwoDee;

/// <summary>
/// The way a animation will loop.
/// </summary>
public enum AnimationLoopType
{
    /// <summary>
    /// The animation will play once.
    /// </summary>
    None = 0,

    /// <summary>
    /// Animation will loop normally, after the last frame is the first frame.
    /// </summary>
    Normal = 1,

    /// <summary>
    /// The animation will play in reverse after reaching then last frame.
    /// </summary>
    NormalThenReverse = 2,

    /// <summary>
    /// The animation will play in reverse.
    /// </summary>
    Reverse = 3,

    /// <summary>
    /// The animation will play once, in reverse.
    /// </summary>
    NoneReverse = 4
}

public class SpriteAnimationData
{
    /// <summary>
    /// The unique name of the animation.
    /// This is how the Controller will refer to it when indexing.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Describes the way the animation will loop once it reaches the final frame.
    /// </summary>
    public AnimationLoopType LoopType { get; set; } = AnimationLoopType.Normal;

    /// <summary>
    /// Frame indices into the frame source that comprise this animation.
    /// </summary>
    public int[] FrameIndices;

    /// <summary>
    /// The delay between frames in milliseconds.
    /// </summary>
    public int TimeBetweenFrames = 150;

    public SpriteAnimationData(string name, int frames)
    {
        Name = name;
        FrameIndices = new int[frames];
    }

    // Serialization constructor.
    protected SpriteAnimationData()
    {
        Name = null!;
        FrameIndices = null!;
    }
}