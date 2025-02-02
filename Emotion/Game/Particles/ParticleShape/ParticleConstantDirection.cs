using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emotion.Game.Particles;
using Emotion.Utility;

namespace Emotion.Game.Particles.ParticleShape;

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

    public void SetParticleDirection(ref Particle particle)
    {
        if (_directionMax != null)
        {
            particle.TargetDirection = Vector3.Lerp(_direction, _directionMax.Value, Helpers.GenerateRandomFloat());
            return;
        }

        particle.TargetDirection = _direction;
    }
}
