using Emotion.Game.Physics2D.Actors;
using Emotion.Game.Physics2D.Collision;

namespace Emotion.Game.Physics2D.Distance
{
    /// <summary>Input for Distance.ComputeDistance(). You have to option to use the shape radii in the computation.</summary>
    public struct DistanceInput
    {
        public DistanceProxy ProxyA;
        public DistanceProxy ProxyB;
        public PhysicsTransform TransformA;
        public PhysicsTransform TransformB;
        public bool UseRadii;
    }
}