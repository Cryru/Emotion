namespace Emotion.Game.Animation
{
    /// <summary>
    /// A single animation node.
    /// </summary>
    public class AnimationNode
    {
        public string Name { get; set; }
        public int StartingFrame { get; set; }
        public int EndingFrame { get; set; } = -1;
        public int TimeBetweenFrames { get; set; } = 500;
        public AnimationLoopType LoopType { get; set; } = AnimationLoopType.Normal;

        public AnimationNode()
        {
        }

        public AnimationNode(string name)
        {
            Name = name;
        }
    }
}