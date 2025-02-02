using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emotion.Game.Particles;

namespace Emotion.Game.Particles.ParticleShape;

public interface IParticleDirectionShape
{
    public void SetParticleDirection(ref Particle particle);
}
