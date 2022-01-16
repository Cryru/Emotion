using System.Numerics;
using Emotion.Game.Physics2D.Actors;
using Emotion.Game.Physics2D.Collision;

namespace Emotion.Game.Physics2D.Distance
{
    /// <summary>Input parameters for b2ShapeCast</summary>
    public struct ShapeCastInput
    {
        public DistanceProxy ProxyA;
        public DistanceProxy ProxyB;
        public PhysicsTransform TransformA;
        public PhysicsTransform TransformB;
        public Vector2 TranslationB;
    }
}