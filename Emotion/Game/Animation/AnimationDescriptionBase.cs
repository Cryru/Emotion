namespace Emotion.Game.Animation
{
    /// <summary>
    /// A description file which can be serialized/deserialized and from which an animated texture can be created.
    /// </summary>
    public abstract class AnimationDescriptionBase
    {
        public string SpriteSheetName { get; set; }
        public int StartingFrame { get; set; }
        public int EndingFrame { get; set; }
        public int TimeBetweenFrames { get; set; }
        public AnimationLoopType LoopType { get; set; }

        public abstract AnimatedTextureBase CreateFrom();
    }
}