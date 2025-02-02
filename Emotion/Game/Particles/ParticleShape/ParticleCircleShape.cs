using Emotion.Utility;

namespace Emotion.Game.Particles.ParticleShape
{
    public class ParticleCircleShape : IParticleDirectionShape
    {
        public bool TowardsCenter = false;

        private Circle _circle;

        public ParticleCircleShape(Circle circle)
        {
            _circle = circle;
        }

        public void SetParticleDirection(Particle particle)
        {
            Vector2 direction = Vector2.Normalize(_circle.Center - particle.Position.ToVec2());
            float angle = MathF.Atan2(direction.Y, direction.X);
            if (angle == 0) angle = Maths.DegreesToRadians(359) * Helpers.GenerateRandomFloat();

            // Vector2 PointOnCircle(float angle)
            Vector3 pointOnLine = _circle.PointOnCircumference(angle).ToVec3();
            particle.TargetDirection = Vector3.Normalize(pointOnLine - particle.Position);
        }

    }
}

