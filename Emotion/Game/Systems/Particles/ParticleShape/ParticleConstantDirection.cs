#nullable enable

namespace Emotion.Game.Systems.Particles.ParticleShape;

public class ParticleConstantDirection : IParticleDirectionShape
{
    private Vector3 _direction;
    private Vector3? _directionMax;

    public ParticleConstantDirection(Vector3 direction)
    {
        _direction = direction;
    }

    public ParticleConstantDirection(Vector3 direction, Vector3 directionMax)
    {
        _direction = direction;
        _directionMax = directionMax;
    }

    public void SetParticleDirection(Particle particle)
    {
        if (_directionMax != null)
        {
            particle.TargetDirection = Vector3.Lerp(_direction, _directionMax.Value, Helpers.GenerateRandomFloat());
            return;
        }

        particle.TargetDirection = _direction;
    }
}
