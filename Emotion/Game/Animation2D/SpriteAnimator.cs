namespace Emotion.Game.Animation2D
{
    public class SpriteAnimator
    {
        public SpriteAnimatorData Data { get; protected set; }

        public SpriteAnimator(SpriteAnimatorData data)
        {
            Data = data;
        }

        public void SetAnimation(string animationName)
        {
        }

        public void Update(float deltaTimeMs)
        {
        }

        public void GetRenderData()
        {
        }
    }
}