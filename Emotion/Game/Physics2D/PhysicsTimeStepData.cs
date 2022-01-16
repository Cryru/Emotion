namespace Emotion.Game.Physics2D
{
    public class PhysicsTimeStepData
    {
        public float DeltaTime;

        public float InvertedDeltaTime
        {
            get => DeltaTime > 0.0f ? 1.0f / DeltaTime : 0.0f;
        }

        public float DeltaTimeRatio;

        public float InvertedDeltaTime0;

        public int PositionIterations = 3;
        public int VelocityIterations = 8;
    }
}