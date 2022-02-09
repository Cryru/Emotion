namespace Emotion.Game.Animation2D
{
    public class SpriteAnimationData
    {
        public string Name { get; set; }

        public SpriteAnimationData(string name)
        {
            Name = name;
        }

        // Serialization constructor.
        protected SpriteAnimationData()
        {

        }
    }
}